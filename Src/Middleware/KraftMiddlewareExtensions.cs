using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.Web.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Recorders.Store;
using Ccf.Ck.Utilities.DependencyContainer;
using Ccf.Ck.Utilities.MemoryCache;
using Ccf.Ck.Utilities.NodeSetService;
using Ccf.Ck.Utilities.Profiling;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using static Ccf.Ck.Utilities.Generic.Utilities;

namespace Ccf.Ck.Web.Middleware
{
    public static class BindKraftExtensions
    {
        static KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings = null;
        static ILogger _Logger = null;
        static IMemoryCache _MemoryCache = null;
        static IConfiguration _Configuration = null;
        static readonly object _SyncRoot = new Object();
        private static readonly List<string> _ValidSubFoldersForWatching = new List<string> { "Css", "Documentation", "Localization", "NodeSets", "Scripts", "Templates", "Views" };

        public static IServiceProvider UseBindKraft(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                services.AddDistributedMemoryCache();
                services.UseBindKraftLogger();
                // If using Kestrel:
                services.Configure<KestrelServerOptions>(options =>
                {
                    options.AllowSynchronousIO = true;
                });

                // If using IIS:
                services.Configure<IISServerOptions>(options =>
                {
                    options.AllowSynchronousIO = true;
                });
                _KraftGlobalConfigurationSettings = new KraftGlobalConfigurationSettings();
                configuration.GetSection("KraftGlobalConfigurationSettings").Bind(_KraftGlobalConfigurationSettings);
                _Configuration = configuration;

                services.AddSingleton(_KraftGlobalConfigurationSettings);

                if (_KraftGlobalConfigurationSettings.GeneralSettings.RedirectToHttps)
                {
                    services.Configure<ForwardedHeadersOptions>(options =>
                    {
                        options.KnownNetworks.Clear(); //its loopback by default
                        options.KnownProxies.Clear();
                        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    });
                    services.AddHsts(options =>
                    {
                        options.Preload = true;
                        options.IncludeSubDomains = true;
                        options.MaxAge = TimeSpan.FromDays(365);
                    });
                    services.AddHttpsRedirection(options =>
                    {
                        options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                        options.HttpsPort = 443;
                    });
                }

                if (_KraftGlobalConfigurationSettings.GeneralSettings.SignalRSettings.UseSignalR)
                {
                    services.AddSignalR(hubOptions =>
                    {
                        hubOptions.KeepAliveInterval = TimeSpan.FromDays(1);
                        hubOptions.EnableDetailedErrors = true;
                        hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(30);
                        hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                    });
                }

                //GRACE DEPENDENCY INJECTION CONTAINER 
                DependencyInjectionContainer dependencyInjectionContainer = new DependencyInjectionContainer();
                services.AddSingleton(dependencyInjectionContainer);
                services.AddRouting(options => options.LowercaseUrls = true);

                services.AddResponseCaching();
                services.AddMemoryCache();
                services.AddSession(options =>
                {
                    // Set a short timeout for easy testing.
                    options.IdleTimeout = TimeSpan.FromMinutes(60);
                    // You might want to only set the application cookies over a secure connection:
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.HttpOnly = true;
                    // Make the session cookie essential
                    options.Cookie.IsEssential = true;
                });
                services.UseBindKraftProfiler();
                IServiceProvider serviceProvider = services.BuildServiceProvider();
                IWebHostEnvironment env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                _Logger = loggerFactory.CreateLogger<KraftMiddleware>();

                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    if (env.IsDevelopment())
                    {
                        loggingBuilder.SetMinimumLevel(LogLevel.Error);
                        loggingBuilder.AddConsole();
                        loggingBuilder.AddDebug();
                    }
                });

                //memory cache
                _MemoryCache = serviceProvider.GetRequiredService<IMemoryCache>();

                /*if (_HostingEnvironment.IsDevelopment())
                {
                    config.CacheProfiles.Add("Default", new CacheProfile() { Location = ResponseCacheLocation.None, Duration = 0 });
                }
                else
                {
                    config.CacheProfiles.Add("Default", new CacheProfile() { Location = ResponseCacheLocation.Any, Duration = 60 });
                }*/

                ICachingService cachingService = new MemoryCachingService(_MemoryCache);
                services.AddSingleton(cachingService);

                KraftModuleCollection kraftModuleCollection = new KraftModuleCollection(_KraftGlobalConfigurationSettings, dependencyInjectionContainer, _Logger);
                services.AddSingleton(kraftModuleCollection);

                #region Global Configuration Settings

                _KraftGlobalConfigurationSettings.GeneralSettings.ReplaceMacrosWithPaths(env.ContentRootPath, env.WebRootPath);
             
                #endregion //Global Configuration Settings

                //INodeSet service
                services.AddSingleton(typeof(INodeSetService), new NodeSetService(_KraftGlobalConfigurationSettings, cachingService));

                ILogger logger = loggerFactory.CreateLogger(env.EnvironmentName);
                services.AddSingleton(typeof(ILogger), logger);

                services.AddSingleton(services);
                #region Authorization
                if (_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RequireAuthorization)
                {
                    services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    })

                    .AddCookie(options =>
                    {
                        options.LoginPath = new PathString("/account/signin");
                    })

                    .AddOpenIdConnect(options =>
                    {
                        // Note: these settings must match the application details
                        // inserted in the database at the server level.
                        options.ClientId = _KraftGlobalConfigurationSettings.GeneralSettings.ClientId;
                        options.ClientSecret = _KraftGlobalConfigurationSettings.GeneralSettings.ClientSecret;
                        options.RequireHttpsMetadata = _KraftGlobalConfigurationSettings.GeneralSettings.RedirectToHttps;
                        options.Authority = _KraftGlobalConfigurationSettings.GeneralSettings.Authority;
                        options.GetClaimsFromUserInfoEndpoint = true;
                        options.SaveTokens = true;

                        // Use the authorization code flow.
                        options.ResponseType = OpenIdConnectResponseType.Code;
                        options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;

                        options.Scope.Add("email");
                        options.Scope.Add("roles");
                        options.Scope.Add("firstname");
                        options.Scope.Add("lastname");
                        options.Scope.Add("offline_access");

                        options.Events = new OpenIdConnectEvents
                        {
                            OnRedirectToIdentityProvider = context =>
                            {
                                string returnUrl = context.HttpContext.Session.GetString("returnurl");//Has returnurl already in user's session
                                if (string.IsNullOrEmpty(returnUrl))
                                {
                                    if (context.Request.Query.ContainsKey("returnurl"))//Is passed as parameter in the url
                                    {
                                        returnUrl = context.Request.Query["returnurl"];
                                    }
                                }
                                if (!string.IsNullOrEmpty(returnUrl))
                                {
                                    context.ProtocolMessage.SetParameter("returnurl", returnUrl);
                                }
                                return Task.CompletedTask;
                            },
                            OnRemoteFailure = context =>
                            {
                                KraftLogger.LogWarning("OnRemoteFailure in KraftMiddlewareExtensions", context.Failure);
                                HttpRequest request = context.Request;
                                foreach (var cookie in context.Request.Cookies)
                                {
                                    if (!cookie.Key.Equals(CookieRequestCultureProvider.DefaultCookieName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        context.Response.Cookies.Delete(cookie.Key);
                                    }
                                }
                                string redirectUrl = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), request.PathBase.ToUriComponent());
                                context.Response.Redirect(redirectUrl);
                                context.HandleResponse();
                                return Task.CompletedTask;
                            },
                            OnAuthenticationFailed = context =>
                            {
                                KraftLogger.LogError("OnAuthenticationFailed in KraftMiddlewareExtensions", context.Exception);
                                HttpRequest request = context.Request;
                                context.Properties.RedirectUri = context.ProtocolMessage.RedirectUri?.Replace("http://", "https://");
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                if (context.ProtocolMessage.Parameters.ContainsKey("returnurl"))//This is coming from the authorization server
                                {
                                    string returnurl = context.ProtocolMessage.Parameters["returnurl"];
                                    context.Properties.RedirectUri = returnurl;
                                    if (!string.IsNullOrEmpty(returnurl))
                                    {
                                        context.HttpContext.Session.SetString("returnurl", returnurl);
                                    }
                                }
                                else if (!string.IsNullOrEmpty(_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RedirectAfterLogin))
                                {
                                    context.Properties.RedirectUri = _KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RedirectAfterLogin;
                                }
                                return Task.CompletedTask;
                            }
                        };
                        options.SecurityTokenValidator = new JwtSecurityTokenHandler
                        {
                            // Disable the built-in JWT claims mapping feature.
                            InboundClaimTypeMap = new Dictionary<string, string>()
                        };
                        options.TokenValidationParameters.NameClaimType = "name";
                        options.TokenValidationParameters.RoleClaimType = "role";
                    });
                }
                else
                {
                    services.AddAuthorization(x =>
                    {
                        x.DefaultPolicy = new AuthorizationPolicyBuilder()
                                .RequireAssertion(_ => true)
                                .Build();
                    });
                }
                #endregion Authorization
                services.UseBundling();
                //Dataprotection
                //This should supress The antiforgery token could not be decrypted error TODO check the logs
                DirectoryInfo dataProtection = new DirectoryInfo(Path.Combine(env.ContentRootPath, "DataProtection"));
                if (!dataProtection.Exists)
                {
                    dataProtection.Create();
                }
                services.AddDataProtection(opts =>
                {
                    opts.ApplicationDiscriminator = "corekraft";
                })
                .PersistKeysToFileSystem(dataProtection)
                .SetDefaultKeyLifetime(TimeSpan.FromDays(10));
                //End Dataprotection

                services.Configure<HubOptions>(options =>
                {
                    options.MaximumReceiveMessageSize = null;
                });

                //Signals
                services.AddHostedService<SignalService>();
                //End Signals
                //RecordersStore which contians dictionary of the running instances
                services.AddSingleton<RecordersStoreImp>();
            }
            catch (Exception ex)
            {
                KraftLogger.LogError("Method: ConfigureServices ", ex);
                KraftExceptionHandlerMiddleware.Exceptions[KraftExceptionHandlerMiddleware.EXCEPTIONSONCONFIGURESERVICES].Add(ex);
            }

            return services.BuildServiceProvider();
        }

        public static IApplicationBuilder UseBindKraft(this IApplicationBuilder app, IWebHostEnvironment env, Action<bool> restart = null)
        {
            //AntiforgeryService
            //app.Use(next => context =>
            //{
            //    if (string.Equals(context.Request.Path.Value, "/", StringComparison.OrdinalIgnoreCase))
            //    {
            //        AntiforgeryTokenSet tokens = app.ApplicationServices.GetService<IAntiforgery>().GetAndStoreTokens(context);
            //        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = false });
            //    }
            //    return next(context);
            //});

            _KraftGlobalConfigurationSettings.EnvironmentSettings = new KraftEnvironmentSettings(env.ApplicationName, env.ContentRootPath, env.EnvironmentName, env.WebRootPath);

            try
            {
                ILoggerFactory loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
                DiagnosticListener diagnosticListener = app.ApplicationServices.GetService<DiagnosticListener>();
                //First statement to register Error handling !!!Keep at the top!!!
                app.UseMiddleware<KraftExceptionHandlerMiddleware>(loggerFactory, new ExceptionHandlerOptions(), diagnosticListener);
                AppDomain.CurrentDomain.UnhandledException += AppDomain_OnUnhandledException;
                AppDomain.CurrentDomain.AssemblyResolve += AppDomain_OnAssemblyResolve;
                if (_KraftGlobalConfigurationSettings.GeneralSettings.RedirectToHttps)
                {
                    app.UseForwardedHeaders();
                    app.UseHsts();
                    app.UseHttpsRedirection();
                }
                if (_KraftGlobalConfigurationSettings.GeneralSettings.RedirectToWww)
                {
                    RewriteOptions rewrite = new RewriteOptions();
                    rewrite.AddRedirectToWwwPermanent();
                    app.UseRewriter(rewrite);
                }
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "wwwroot")),
                    ServeUnknownFileTypes = true,
                    RequestPath = new PathString(string.Empty),
                });

                ExtensionMethods.Init(app, _Logger);
                ToolSettings tool = KraftToolsRouteBuilder.GetTool(_KraftGlobalConfigurationSettings, "errors");
                string segment = null;
                if (tool != null && tool.Enabled)//Errors enabled from configuration
                {
                    segment = tool.Url;
                }
                app.UseBindKraftLogger(env, loggerFactory, segment);
                app.UseBindKraftProfiler(env, loggerFactory, _MemoryCache);
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                BundleCollection bundleCollection = app.UseBundling(env, loggerFactory.CreateLogger("Bundling"), _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlCssJsSegment, _KraftGlobalConfigurationSettings.GeneralSettings.EnableOptimization);
                bundleCollection.EnableInstrumentations = env.IsDevelopment(); //Logging enabled 

                #region Initial module registration
                foreach (string dir in _KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders)
                {
                    if (!Directory.Exists(dir))
                    {
                        throw new Exception($"No \"{dir}\" directory found! The CoreKraft initialization cannot continue.");
                    }
                }
                string kraftUrlSegment = _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlSegment;
                try
                {
                    KraftModuleCollection modulesCollection = app.ApplicationServices.GetService<KraftModuleCollection>();
                    Dictionary<string, string> moduleKey2Path = new Dictionary<string, string>();
                    IHostApplicationLifetime applicationLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
                    lock (_SyncRoot)
                    {
                        foreach (string dir in _KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders)
                        {
                            string[] moduleDirectories = Directory.GetDirectories(dir);
                            foreach (string subdirectory in moduleDirectories)
                            {
                                DirectoryInfo moduleDirectory = new DirectoryInfo(subdirectory);
                                if (moduleDirectory.Name != null && moduleDirectory.Name.Equals("_PluginsReferences", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    //if (env.IsDevelopment())
                                    //{
                                    //    AttachModulesWatcher(moduleDirectory.FullName, false, applicationLifetime, restart);
                                    //}
                                    continue;
                                }
                                ICachingService cachingService = app.ApplicationServices.GetService<ICachingService>();
                                KraftModule kraftModule = modulesCollection.GetModule(moduleDirectory.Name);
                                if (kraftModule != null)
                                {
                                    continue;
                                }
                                kraftModule = modulesCollection.RegisterModule(dir, moduleDirectory.Name, cachingService);
                                if (kraftModule == null || !kraftModule.IsInitialized)
                                {
                                    _Logger.LogInformation($"Module not created for directory \"{moduleDirectory.Name}\" because of missing configuration files.");
                                    continue;
                                }
                                KraftStaticFiles.RegisterStaticFiles(app, moduleDirectory.FullName, kraftUrlSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlResourceSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlModuleImages);
                                KraftStaticFiles.RegisterStaticFiles(app, moduleDirectory.FullName, kraftUrlSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlResourceSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlModulePublic);
                                moduleKey2Path.Add(kraftModule.Key.ToLower(), dir);
                                //The application will restart when some files changed in the modules directory and subdirectories but only in RELEASE
                                //Check if module is initialized Robert
                                if (kraftModule.IsInitialized)
                                {
                                    string moduleFullPath = Path.Combine(dir, kraftModule.Key);
                                    string path2Data = Path.Combine(moduleFullPath, "Data");
                                    if (!HasWritePermissionOnDir(new DirectoryInfo(path2Data), true))
                                    {
                                        throw new SecurityException($"Write access to folder {path2Data} is required!");
                                    }
                                    path2Data = Path.Combine(moduleFullPath, "Images");
                                    if (!HasWritePermissionOnDir(new DirectoryInfo(path2Data), true))
                                    {
                                        throw new SecurityException($"Write access to folder {path2Data} is required!");
                                    }
                                    foreach (string validSubFolder in _ValidSubFoldersForWatching)
                                    {
                                        AttachModulesWatcher(Path.Combine(moduleFullPath, validSubFolder), true, applicationLifetime, restart);
                                    }
                                }
                            }
                        }
                        //try to construct all modules
                        modulesCollection.ResolveModuleDependencies();
                        _KraftGlobalConfigurationSettings.GeneralSettings.ModuleKey2Path = moduleKey2Path;
                    }
                    #region Watching appsettings, PassThroughJsConfig, nlogConfig
                    //appsettings.{Production} configuration watch
                    _Configuration.GetReloadToken().RegisterChangeCallback(_ =>
                    {
                        string environment = "Production";
                        if (env.IsDevelopment())
                        {
                            environment = "Development";
                        }
                        RestartReason restartReason = new RestartReason
                        {
                            Reason = "appsettings-Configuration Changed",
                            Description = $"'appsettings.{environment}.json' has been altered"
                        };
                        AppDomain.CurrentDomain.UnhandledException -= AppDomain_OnUnhandledException;
                        AppDomain.CurrentDomain.AssemblyResolve -= AppDomain_OnAssemblyResolve;
                        RestartApplication(applicationLifetime, restartReason, restart);
                    }, null);
                    //PassThroughJsConfig configuration watch
                    IChangeToken changeTokenPassThroughJsConfig = _KraftGlobalConfigurationSettings.GeneralSettings.BindKraftConfigurationGetReloadToken(env);
                    if (changeTokenPassThroughJsConfig != null)
                    {
                        changeTokenPassThroughJsConfig.RegisterChangeCallback(_ =>
                        {
                            RestartReason restartReason = new RestartReason
                            {
                                Reason = "PassThroughJsConfig Changed",
                                Description = $"'{_KraftGlobalConfigurationSettings.GeneralSettings.PassThroughJsConfig}' has been altered"
                            };
                            AppDomain.CurrentDomain.UnhandledException -= AppDomain_OnUnhandledException;
                            AppDomain.CurrentDomain.AssemblyResolve -= AppDomain_OnAssemblyResolve;
                            RestartApplication(applicationLifetime, restartReason, restart);
                        }, null);
                    }
                    FileInfo nlogConfig = new FileInfo(Path.Combine(env.ContentRootPath, "nlog.config"));
                    if (nlogConfig.Exists)
                    {
                        IChangeToken changeTokenNlogConfig = env.ContentRootFileProvider.Watch(nlogConfig.Name);
                        if (changeTokenNlogConfig != null)
                        {
                            changeTokenNlogConfig.RegisterChangeCallback(_ =>
                            {
                                RestartReason restartReason = new RestartReason
                                {
                                    Reason = "Nlog.config Changed",
                                    Description = $"'Nlog.config' has been altered"
                                };
                                AppDomain.CurrentDomain.UnhandledException -= AppDomain_OnUnhandledException;
                                AppDomain.CurrentDomain.AssemblyResolve -= AppDomain_OnAssemblyResolve;
                                RestartApplication(applicationLifetime, restartReason, restart);
                            }, null);
                        }
                    }
                    #endregion End: Watching appsettings, PassThroughJsConfig, nlogConfig
                }
                catch (Exception boom)
                {
                    KraftLogger.LogError(boom);
                    throw new Exception($"CoreKrafts module construction failed! {boom.Message}");
                }
                #endregion Initial module registration
                //Configure the CoreKraft routing               
                RouteHandler kraftRoutesHandler = new RouteHandler(KraftMiddleware.ExecutionDelegate(app, _KraftGlobalConfigurationSettings));
                app.UseRouter(KraftRouteBuilder.MakeRouter(app, kraftRoutesHandler, kraftUrlSegment));

                #region Tools routing
                KraftToolsRouteBuilder.MakeRouters(app, _KraftGlobalConfigurationSettings);
                #endregion Tools routing

                DirectCallService.Instance.Call = KraftMiddleware.ExecutionDelegateDirect(app, _KraftGlobalConfigurationSettings);
                app.UseSession();
                if (_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RequireAuthorization)
                {
                    app.UseAuthentication();
                }
                //KraftKeepAlive.RegisterKeepAliveAsync(builder);
                //Configure eventually SignalR
                try
                {
                    if (_KraftGlobalConfigurationSettings.GeneralSettings.SignalRSettings.UseSignalR)
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            MethodInfo mapHub = typeof(HubEndpointRouteBuilderExtensions).GetMethod(
                                "MapHub", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IEndpointRouteBuilder), typeof(string), typeof(Action<HttpConnectionDispatcherOptions>) }, null);
                            MethodInfo generic = mapHub.MakeGenericMethod(Type.GetType(_KraftGlobalConfigurationSettings.GeneralSettings.SignalRSettings.HubImplementationAsString, true));
                            generic.Invoke(null,
                                new object[] { endpoints, new string(_KraftGlobalConfigurationSettings.GeneralSettings.SignalRSettings.HubRoute),
                                (Action<HttpConnectionDispatcherOptions>)(x => {x.ApplicationMaxBufferSize = 3200000; x.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30); x.LongPolling.PollTimeout = TimeSpan.FromSeconds(180); })
                            });
                        });
                    }
                }
                catch (Exception e)
                {
                    KraftLogger.LogError("Register signalR middleware. Exception: " + e);
                }
                //Signals
                SignalStartup signalStartup = new SignalStartup(app.ApplicationServices, _KraftGlobalConfigurationSettings);
                signalStartup.ExecuteSignalsOnStartup();
                //End Signals
            }
            catch (Exception ex)
            {
                KraftLogger.LogError("Method: UseBindKraft ", ex);
                KraftExceptionHandlerMiddleware.Exceptions[KraftExceptionHandlerMiddleware.EXCEPTIONSONCONFIGURE].Add(ex);
            }

            //This is the last statement
            KraftExceptionHandlerMiddleware.HandleErrorAction(app);
            return app;
        }

        private static void AppDomain_OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            KraftLogger.LogCritical("AppDomain_OnUnhandledException: ", e.ExceptionObject);
        }

        private static Assembly AppDomain_OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources
            if (args.Name.Contains(".resources"))
            {
                return null;
            }

            // check for assemblies already loaded
            string[] nameParts = args.Name.Split(',');
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a =>
            {
                string[] parts = a.FullName.Split(',');
                return parts[0].Equals(nameParts[0], StringComparison.InvariantCultureIgnoreCase);
            }
            );
            if (assembly != null)
            {
                return assembly;
            }

            // Try to load by filename - split out the filename of the full assembly name
            // and append the base path of the original assembly (ie. look in the same dir)
            string fileName = nameParts[0] + ".dll".ToLower();
            bool found = false;
            foreach (string dir in _KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders)
            {
                string asmFullName = Path.Combine(dir, "_PluginsReferences", fileName);

                try
                {
                    Assembly loadedAssembly = Assembly.LoadFile(asmFullName);
                    if (loadedAssembly != null)
                    {
                        found = true;
                        return loadedAssembly;
                    }
                }
                catch
                {
                    //do nothing
                    continue;
                }
                finally
                {
                    if (!found)
                    {
                        KraftLogger.LogError($"Method: CurrentDomain_AssemblyResolve: The file {asmFullName} requested by {args.RequestingAssembly.FullName} was not found!");
                    }
                }
            }
            return null;
        }

        private static void AttachModulesWatcher(string moduleFolder, bool includeSubdirectories, IHostApplicationLifetime applicationLifetime, Action<bool> restart)
        {
            if (!Directory.Exists(moduleFolder))
            {
                //Do nothing for none existant folders
                return;
            }
            FileSystemWatcher fileWatcher;
            RestartReason restartReason = new RestartReason();
            fileWatcher = new FileSystemWatcher(moduleFolder)
            {
                // watch for everything
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = includeSubdirectories,
                Filter = "*.*"
            };
            // Add event handlers.
            fileWatcher.Changed += OnChanged;
            fileWatcher.Created += OnChanged;
            fileWatcher.Deleted += OnChanged;
            fileWatcher.Renamed += OnRenamed;

            // Begin watching...
            fileWatcher.EnableRaisingEvents = true;


            void OnChanged(object source, FileSystemEventArgs e)
            {
                fileWatcher.EnableRaisingEvents = false;
                //Bug in Docker which will trigger OnChanged during StartUp (How to fix?)
                AppDomain.CurrentDomain.UnhandledException -= AppDomain_OnUnhandledException;
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomain_OnAssemblyResolve;
                restartReason.Reason = "File Changed";
                restartReason.Description = $"ChangeType: {e.ChangeType} file {e.FullPath}";
                RestartApplication(applicationLifetime, restartReason, restart);
            }

            void OnRenamed(object source, RenamedEventArgs e)
            {
                fileWatcher.EnableRaisingEvents = false;
                AppDomain.CurrentDomain.UnhandledException -= AppDomain_OnUnhandledException;
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomain_OnAssemblyResolve;
                restartReason.Reason = "File Renamed";
                restartReason.Description = $"Renaming from {e.OldFullPath} to {e.FullPath}";
                RestartApplication(applicationLifetime, restartReason, restart);
            }
        }
    }
}

