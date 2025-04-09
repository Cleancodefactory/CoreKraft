using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.Web.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.Packet;
using Ccf.Ck.SysPlugins.Recorders.Store;
using Ccf.Ck.Utilities.CookieTicketStore;
using Ccf.Ck.Utilities.CookieTicketStore.CookieSerialization;
using Ccf.Ck.Utilities.CookieTicketStore.InMemory;
using Ccf.Ck.Utilities.CookieTicketStore.Sqlite;
using Ccf.Ck.Utilities.DependencyContainer;
using Ccf.Ck.Utilities.Generic.Topologies;
using Ccf.Ck.Utilities.MemoryCache;
using Ccf.Ck.Utilities.NodeSetService;
using Ccf.Ck.Utilities.Profiling;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
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
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
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
        private const string _PLUGINSREFERENCES = "_PluginsReferences";
        private static List<Assembly> _WebApiAssemblies = new List<Assembly>();

        public static IServiceProvider UseBindKraft(this IServiceCollection services, IConfiguration configuration)
        {
            KraftLogger.LogInformation($"IServiceProvider UseBindKraft: executed");
            try
            {
                //Register as early as possible
                AppDomain.CurrentDomain.UnhandledException += AppDomain_OnUnhandledException;
                AppDomain.CurrentDomain.AssemblyResolve += AppDomain_OnAssemblyResolve;
                _KraftGlobalConfigurationSettings = new KraftGlobalConfigurationSettings();
                configuration.GetSection("KraftGlobalConfigurationSettings").Bind(_KraftGlobalConfigurationSettings);
                ModelConstants._STATE_PROPERTY_DONOT_CHANGE = _KraftGlobalConfigurationSettings.GeneralSettings.DontSetState;
                if (!string.IsNullOrWhiteSpace(_KraftGlobalConfigurationSettings.GeneralSettings.DataStatePropertyName))
                {
                    ModelConstants._STATE_PROPERTY_NAME = _KraftGlobalConfigurationSettings.GeneralSettings.DataStatePropertyName;
                }
                if (_KraftGlobalConfigurationSettings.GeneralSettings.CorsAllowedOrigins.Enabled)
                {
                    CorsAllowedOrigins cors = _KraftGlobalConfigurationSettings.GeneralSettings.CorsAllowedOrigins;
                    //Cors as early as possible
                    services.AddCors(options =>
                    {
                        options.AddPolicy("CorsPolicy", policyBuilder =>
                        {
                            policyBuilder.SetIsOriginAllowedToAllowWildcardSubdomains();
                            if (cors.WithOrigins.Count > 0)
                            {
                                policyBuilder.WithOrigins(cors.WithOrigins.ToArray());
                                policyBuilder.AllowCredentials();
                            }
                            else
                            {
                                policyBuilder.AllowAnyOrigin();
                            }
                            policyBuilder.AllowAnyMethod();
                            policyBuilder.AllowAnyHeader();
                        });
                    });
                    //Cors
                }
                services.AddDistributedMemoryCache();
                services.UseBindKraftLogger();
                // If using Kestrel:
                services.Configure<KestrelServerOptions>(options =>
                {
                    options.AllowSynchronousIO = true;
                });

                List<IConfigurationSection> slaveConfigSections = new List<IConfigurationSection>();
                foreach (string sectionName in _KraftGlobalConfigurationSettings.GeneralSettings.SlaveConfiguration.Sections)
                {
                    slaveConfigSections.Add(configuration.GetSection(sectionName));
                }
                _Configuration = configuration;

                services.AddSingleton(_KraftGlobalConfigurationSettings);
                services.AddSingleton(slaveConfigSections);

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
                        SignalRHubOptions confHubOptions = _KraftGlobalConfigurationSettings.GeneralSettings.SignalRSettings.SignalRHubOptions;
                        hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(confHubOptions.ClientTimeoutInterval);
                        hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(confHubOptions.KeepAliveInterval);
                        hubOptions.EnableDetailedErrors = confHubOptions.EnableDetailedErrors;
                        hubOptions.MaximumReceiveMessageSize = confHubOptions.MaximumReceiveMessageSize;
                        hubOptions.StreamBufferCapacity = confHubOptions.StreamBufferCapacity; 
                        hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(confHubOptions.HandshakeTimeout);
                        hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(confHubOptions.ClientTimeoutInterval);
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
                ToolSettings tool = KraftToolsRouteBuilder.GetTool(_KraftGlobalConfigurationSettings, "profiler");
                if (tool != null && tool.Enabled)//Profiler enabled enabled from configuration
                {
                    services.UseBindKraftProfiler(tool.Url);
                }

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

                if (_KraftGlobalConfigurationSettings.GeneralSettings.WebApiAreaAssembly.IsConfigured)
                {
                    bool atLeastOneLoaded = false;
                    foreach (string rootFolder in _KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders)
                    {
                        string rootPath = Path.Combine(rootFolder, "_PluginsReferences");
                        foreach (string assemblyName in _KraftGlobalConfigurationSettings.GeneralSettings.WebApiAreaAssembly.AssemblyNames)
                        {
                            string fullName = Path.Combine(rootPath, assemblyName);
                            if (File.Exists(fullName))
                            {
                                Assembly assembly = Assembly.LoadFrom(fullName);
                                _WebApiAssemblies.Add(assembly);

                                if (assembly != null)
                                {
                                    if (_KraftGlobalConfigurationSettings.GeneralSettings.WebApiAreaAssembly.EnableSwagger)
                                    {
                                        RegisterServicesFromAssembly(services, assembly);
                                    }
                                    atLeastOneLoaded = true;
                                    // Find and register the services (could be handy if we have a common interface
                                    //var types = assembly.GetTypes();
                                    //foreach (var type in types)
                                    //{
                                    //    // Register services that implement a specific interface or have a specific attribute
                                    //    if (typeof(IService).IsAssignableFrom(type))
                                    //    {
                                    //        services.AddSingleton(typeof(IService), type);
                                    //    }
                                    //}
                                    // Add controllers from the loaded assembly
                                    services.AddControllers().AddApplicationPart(assembly);
                                }
                            }
                        }
                    }
                    if (atLeastOneLoaded)
                    {
                        services.AddEndpointsApiExplorer();
                    }
                }

                if (_KraftGlobalConfigurationSettings.GeneralSettings.SpaSettings.Enabled)
                {
                    services.AddSpaStaticFiles(configuration =>
                    {
                        configuration.RootPath = _KraftGlobalConfigurationSettings.GeneralSettings.SpaSettings.RootPath;//"wwwroot/search-app";
                    });
                }
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
                        if (_KraftGlobalConfigurationSettings.GeneralSettings.CookieStoreSection.EnableCookieSizeReduction)
                        {
                            IDataProtectionProvider dataProtectionProvider = DataProtectionProvider.Create("CoreKraft");
                            IDataProtector dataProtector = dataProtectionProvider.CreateProtector("Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware");
                            options.TicketDataFormat = new OptimizedTicketDataFormat(loggerFactory, dataProtector);
                        }
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
                            #region Additional OpenIdEvents
                            //OnAccessDenied = context =>
                            //{
                            //    return Task.CompletedTask;
                            //},
                            //OnAuthorizationCodeReceived = context =>
                            //{
                            //    return Task.CompletedTask;
                            //},
                            //OnMessageReceived = context =>
                            //{
                            //    return Task.CompletedTask;
                            //},
                            //OnRedirectToIdentityProviderForSignOut = context =>
                            //{
                            //    return Task.CompletedTask;
                            //},
                            //OnRemoteSignOut = context =>
                            //{
                            //    return Task.CompletedTask;
                            //},
                            //OnSignedOutCallbackRedirect = context =>
                            //{
                            //    return Task.CompletedTask;
                            //},
                            //OnTicketReceived = context =>
                            //{
                            //    // ensure we have an identity
                            //    ClaimsIdentity identity = context.Principal.Identity as ClaimsIdentity;
                            //    if (identity != null)
                            //    {

                            //        //var claims = identity.Claims.Where(claim =>
                            //        //{
                            //        //    if (claim.Type != null && claim.Type == "")
                            //        //    {
                            //        //        return true;
                            //        //    }
                            //        //    if (claim.Type != null && claim.Type == "")
                            //        //    {
                            //        //        return true;
                            //        //    }
                            //        //    return false;
                            //        //});

                            //        //identity.RemoveClaim(...);
                            //    }
                            //    return Task.CompletedTask;
                            //},
                            //OnTokenResponseReceived = context =>
                            //{
                            //    return Task.CompletedTask;
                            //},
                            //OnUserInformationReceived = context =>
                            //{
                            //    return Task.CompletedTask;
                            //},
                            #endregion Additional OpenIdEvents
                            OnRedirectToIdentityProvider = context =>
                            {
                                //Has returnurl already in user's session
                                //Method: static void KraftResult(HttpContext httpContext, HttpStatusCode statusCode, string error = null)
                                if (context != null)
                                {
                                    string returnUrl = context?.Properties?.RedirectUri;
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
                                    context.ProtocolMessage.SetParameter("lang", Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName);
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
                                try
                                {
                                    KraftLogger.LogError("OnAuthenticationFailed in KraftMiddlewareExtensions", context.Exception);
                                    if (context.Exception is OpenIdConnectProtocolException)
                                    {
                                        context.HandleResponse();
                                        context.Response.Redirect("/acount/signin");
                                    }
                                    else
                                    {
                                        context.Properties.RedirectUri = context.ProtocolMessage.RedirectUri?.Replace("http://", "https://");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    KraftLogger.LogError("Exception: OnAuthenticationFailed in KraftMiddlewareExtensions: OpenIdConnectProtocolException", ex);
                                    throw;
                                }
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                string returnurl = null; // = _KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RedirectAfterLogin;
                                if (context.ProtocolMessage.Parameters.ContainsKey("returnurl"))//This is coming from the authorization server
                                {
                                    returnurl = context.ProtocolMessage.Parameters["returnurl"];
                                }
                                if (!string.IsNullOrEmpty(returnurl))
                                {
                                    context.Properties.RedirectUri = returnurl;
                                    //context.HttpContext.Session.SetString("returnurl", returnurl);
                                }

                                return Task.CompletedTask;
                            }
                        };
                        options.TokenHandler = new JwtSecurityTokenHandler
                        {
                            // Disable the built-in JWT claims mapping feature.
                            InboundClaimTypeMap = new Dictionary<string, string>()
                        };
                        options.UseSecurityTokenValidator = true;
                        options.TokenValidationParameters.NameClaimType = "name";
                        options.TokenValidationParameters.RoleClaimType = "role";
                    });
                    if (_KraftGlobalConfigurationSettings.GeneralSettings.CookieStoreSection.CookieStore.IsInMemoryTicketStore)
                    {
                        services.AddTransient<ITicketStore, InMemoryTicketStore>();
                        services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();
                    }
                    else if (_KraftGlobalConfigurationSettings.GeneralSettings.CookieStoreSection.CookieStore.IsSqliteTicketStore)
                    {
                        services.AddTransient<ITicketStore, SqliteTicketStore>();
                        services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieAuthenticationOptions>();
                    }

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
                services.AddSingleton<SignalService>();
                services.AddHostedService<SignalService>(sp => sp.GetRequiredService<SignalService>());
                //End Signals

                var icsvc = new IndirectCallService(null, _KraftGlobalConfigurationSettings);
                services.AddSingleton<IIndirectCallService>(icsvc);
                services.AddHostedService<IndirectCallService>(sp => sp.GetRequiredService<IIndirectCallService>() as IndirectCallService);

                //RecordersStore which contians dictionary of the running instances
                services.AddSingleton<RecordersStoreImp>();
            }
            catch (Exception ex)
            {
                KraftLogger.LogError("Method: ConfigureServices ", ex);
                KraftExceptionHandlerMiddleware.Exceptions[KraftExceptionHandlerMiddleware.EXCEPTIONSONCONFIGURESERVICES].Add(ex);
                throw;
            }

            return services.BuildServiceProvider();
        }

        public static IApplicationBuilder UseBindKraft(this IApplicationBuilder app, IWebHostEnvironment env, Action<bool> restart = null)
        {
            KraftLogger.LogInformation($"IApplicationBuilder UseBindKraft: executed");
            if (_KraftGlobalConfigurationSettings.GeneralSettings.CorsAllowedOrigins.Enabled)
            {
                app.UseOptions();
            }
            //AntiforgeryService
            //app.Use(next => context =>
            //{
            //    if (string.Equals(context.Request.Path.Value, "/", StringComparison.OrdinalIgnoreCase))
            //    {
            //        AntiforgeryTokenSet tokens = app.ApplicationServices.GetService<IAntiforgery>().GetAndStoreTokens(context);
            //        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions() { HttpOnly = true, Secure = true, IsEssential = true, SameSite=SameSiteMode.Strict });
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
                ICorsService corsService = null;
                ICorsPolicyProvider corsPolicyProvider = null;
                if (_KraftGlobalConfigurationSettings.GeneralSettings.CorsAllowedOrigins.Enabled)
                {
                    corsService = app.ApplicationServices.GetService<ICorsService>();
                    corsPolicyProvider = app.ApplicationServices.GetService<ICorsPolicyProvider>();
                }

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "wwwroot")),
                    ServeUnknownFileTypes = true,
                    RequestPath = new PathString(string.Empty),
                    OnPrepareResponse = ctx =>
                    {
                        if (_KraftGlobalConfigurationSettings.GeneralSettings.ProgressiveWebApp != null)
                        {
                            if (!string.IsNullOrEmpty(_KraftGlobalConfigurationSettings.GeneralSettings.ProgressiveWebApp.ServiceWorkerUrl))
                            {
                                if (ctx.File.Name.Equals(_KraftGlobalConfigurationSettings.GeneralSettings.ProgressiveWebApp.ServiceWorkerUrl.Replace("/", string.Empty), StringComparison.OrdinalIgnoreCase))
                                {
                                    ctx.Context.Response.Headers.Append("Cache-Control", "max-age=0, private, no-cache");
                                }
                            }
                        }
                        if (corsPolicyProvider != null)
                        {
                            CorsPolicy policy = corsPolicyProvider.GetPolicyAsync(ctx.Context, "CorsPolicy")
                                                .ConfigureAwait(false)
                                                .GetAwaiter().GetResult();
                            CorsResult corsResult = corsService.EvaluatePolicy(ctx.Context, policy);
                            corsService.ApplyResult(corsResult, ctx.Context.Response);
                        }
                    }
                });

                ExtensionMethods.Init(app, _Logger);

                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                string rootVirtualPath = "/Modules";
                BundleCollection bundleCollection = app.UseBundling(env,
                    _KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders,
                    rootVirtualPath,
                    loggerFactory.CreateLogger("Bundling"),
                    _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlCssJsSegment,
                    _KraftGlobalConfigurationSettings.GeneralSettings.EnableOptimization);
                bundleCollection.EnableInstrumentations = env.IsDevelopment(); //Logging enabled 

                #region Initial module registration
                foreach (string dir in _KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders)
                {
                    if (!Directory.Exists(dir))
                    {
                        throw new Exception($"No \"{dir}\" directory found in the setting ModulesRootFolders! The CoreKraft initialization cannot continue.");
                    }
                }
                string kraftUrlSegment = _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlSegment;
                try
                {
                    KraftModuleCollection modulesCollection = app.ApplicationServices.GetService<KraftModuleCollection>();
                    IHostApplicationLifetime applicationLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
                    lock (_SyncRoot)
                    {
                        KraftModulesConstruction kraftModulesConstruction = new KraftModulesConstruction();
                        Dictionary<string, IDependable<KraftDependableModule>> kraftDependableModules = kraftModulesConstruction.Init(_KraftGlobalConfigurationSettings.GeneralSettings.DefaultStartModule, _KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders);

                        ICachingService cachingService = app.ApplicationServices.GetService<ICachingService>();
                        Dictionary<string, string> moduleKey2Path = new Dictionary<string, string>();
                        SignalService signalService = app.ApplicationServices.GetService<SignalService>();
                        foreach (KeyValuePair<string, IDependable<KraftDependableModule>> depModule in kraftDependableModules)
                        {
                            KraftDependableModule kraftDependable = (depModule.Value as KraftDependableModule);
                            KraftModule kraftModule = modulesCollection.RegisterModule(kraftDependable.KraftModuleRootPath, depModule.Value.Name, kraftDependable, cachingService);
                            KraftStaticFiles.RegisterStaticFiles(app, kraftModule.ModulePath, kraftUrlSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlResourceSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlModuleImages, corsService, corsPolicyProvider);
                            KraftStaticFiles.RegisterStaticFiles(app, kraftModule.ModulePath, kraftUrlSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlResourceSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlModulePublic, corsService, corsPolicyProvider);
                            moduleKey2Path.Add(kraftModule.Key, kraftDependable.KraftModuleRootPath);
                            string moduleFullPath = Path.Combine(kraftDependable.KraftModuleRootPath, kraftModule.Name);
                            string path2Data = Path.Combine(moduleFullPath, "Data");
                            if (!HasWritePermissionOnDir(new DirectoryInfo(path2Data), !env.IsDevelopment()))
                            {
                                throw new SecurityException($"Write access to folder {path2Data} is required!");
                            }
                            path2Data = Path.Combine(moduleFullPath, "Images");
                            if (!HasWritePermissionOnDir(new DirectoryInfo(path2Data), !env.IsDevelopment()))
                            {
                                throw new SecurityException($"Write access to folder {path2Data} is required!");
                            }
                            foreach (string validSubFolder in _KraftGlobalConfigurationSettings.GeneralSettings.WatchSubFoldersForRestart.Count > 0 ? _KraftGlobalConfigurationSettings.GeneralSettings.WatchSubFoldersForRestart : _ValidSubFoldersForWatching)
                            {
                                signalService.AttachModulesWatcher(Path.Combine(moduleFullPath, validSubFolder), true, applicationLifetime, restart, AppDomain_OnUnhandledException, AppDomain_OnAssemblyResolve);
                            }
                        }
                        //Restart when changes in the _PluginsReferences
                        if (env.IsDevelopment())
                        {
                            foreach (string fullPathToModuleDir in _KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders)
                            {
                                string pluginReferences = Path.Combine(fullPathToModuleDir, _PLUGINSREFERENCES);
                                if (Directory.Exists(pluginReferences))
                                {
                                    signalService.AttachModulesWatcher(pluginReferences, false, applicationLifetime, restart, AppDomain_OnUnhandledException, AppDomain_OnAssemblyResolve);
                                }
                            }
                        }
                        _KraftGlobalConfigurationSettings.GeneralSettings.ModuleKey2Path = moduleKey2Path;
                    }
                    #region Watching appsettings, PassThroughJsConfig, nlogConfig
                    string environment = "Production";
                    if (env.IsDevelopment())
                    {
                        environment = "Development";
                    }
                    //Configuration watch PassThroughJsConfig
                    AttachWatcher(env.ContentRootPath, $"appsettings.{environment}.json", applicationLifetime, restart, AppDomain_OnUnhandledException, AppDomain_OnAssemblyResolve);
                    //Configuration watch PassThroughJsConfig
                    if (_KraftGlobalConfigurationSettings.GeneralSettings.GetBindKraftConfigurationContent(env))
                    {
                        AttachWatcher(env.ContentRootPath, _KraftGlobalConfigurationSettings.GeneralSettings.PassThroughJsConfig, applicationLifetime, restart, AppDomain_OnUnhandledException, AppDomain_OnAssemblyResolve);
                    }
                    //Configuration watch nlog.config
                    AttachWatcher(env.ContentRootPath, "nlog.config", applicationLifetime, restart, AppDomain_OnUnhandledException, AppDomain_OnAssemblyResolve);
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
                ToolSettings tool = KraftToolsRouteBuilder.GetTool(_KraftGlobalConfigurationSettings, "errors");
                string segment = null;
                if (tool != null && tool.Enabled)//Errors enabled from configuration
                {
                    segment = tool.Url;
                }
                app.UseBindKraftLogger(env, loggerFactory, segment);
                KraftToolsRouteBuilder.MakeRouters(app, _KraftGlobalConfigurationSettings);
                #endregion Tools routing

                DirectCallService.Instance.Call = KraftMiddleware.ExecutionDelegateDirect(app, _KraftGlobalConfigurationSettings);
                app.UseSession();
                if (_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RequireAuthorization)
                {
                    app.UseAuthentication();
                }
                app.UseRouting();
                if (_KraftGlobalConfigurationSettings.GeneralSettings.SpaSettings.Enabled)
                {
                    app.Use(async (context, next) =>
                    {
                        System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> found = context.Request.Query.FirstOrDefault(kv =>
                        {
                            if (kv.Key.Contains("callsignin", StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                            return false;
                        });
                        if (found.Key != null)
                        {
                            context.Response.Redirect("/account/signin");
                            return;
                        }
                        await next(context);
                    });
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllerRoute(
                        name: "signin",
                        pattern: "account/signin/{*all}",
                        defaults: new { controller = "Account", action = "SignIn" });

                        endpoints.MapControllerRoute(
                        name: "signout",
                        pattern: "account/signout",
                        defaults: new { controller = "Account", action = "SignOut" });
                    });

                    app.UseSpaStaticFiles();
                    app.UseSpa(spa =>
                    {
                        spa.Options.SourcePath = _KraftGlobalConfigurationSettings.GeneralSettings.SpaSettings.SourcePath;//"search-app";
                    });
                }
                if (_KraftGlobalConfigurationSettings.GeneralSettings.WebApiAreaAssembly.IsConfigured)
                {
                    foreach (Assembly webApiAssembly in _WebApiAssemblies)
                    {
                        if (_KraftGlobalConfigurationSettings.GeneralSettings.WebApiAreaAssembly.EnableSwagger)
                        {
                            RegisterBuilderFromAssembly(app, webApiAssembly);
                        }
                    }
                }
                //KraftKeepAlive.RegisterKeepAliveAsync(builder);
                //Configure eventually SignalR
                try
                {
                    if (_KraftGlobalConfigurationSettings.GeneralSettings.SignalRSettings.UseSignalR)
                    {
                        app.UseEndpoints(endpoints =>
                        {
                            MethodInfo mapHub = typeof(HubEndpointRouteBuilderExtensions).GetMethod(
                                "MapHub", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(IEndpointRouteBuilder), typeof(string), typeof(Action<HttpConnectionDispatcherOptions>) }, null);
                            MethodInfo generic = mapHub.MakeGenericMethod(Type.GetType(_KraftGlobalConfigurationSettings.GeneralSettings.SignalRSettings.HubImplementationAsString, true));
                            generic.Invoke(null,
                                new object[] { endpoints, new string(_KraftGlobalConfigurationSettings.GeneralSettings.SignalRSettings.HubRoute),
                                (Action<HttpConnectionDispatcherOptions>)(x => {
                                    x.ApplicationMaxBufferSize = _KraftGlobalConfigurationSettings.GeneralSettings.SignalRSettings.SignalRHttpConnectionOptions.ApplicationMaxBufferSize;
                                    x.TransportMaxBufferSize = _KraftGlobalConfigurationSettings.GeneralSettings.SignalRSettings.SignalRHttpConnectionOptions.TransportMaxBufferSize;
                                    x.WebSockets.CloseTimeout = TimeSpan.FromSeconds(30);
                                    x.LongPolling.PollTimeout = TimeSpan.FromSeconds(180); 
                                })
                            });
                            if (_KraftGlobalConfigurationSettings.GeneralSettings.HistoryNavSettings.Enabled)
                            {
                                if (_KraftGlobalConfigurationSettings.GeneralSettings.SpaSettings.Enabled)
                                {
                                    throw new Exception("HistoryNavSettings can't be enabled when the SpaSettings are Enabled. Please correct and restart.");
                                }
                                endpoints.MapControllerRoute(
                                name: _KraftGlobalConfigurationSettings.GeneralSettings.HistoryNavSettings.Name,//"nav"
                                pattern: _KraftGlobalConfigurationSettings.GeneralSettings.HistoryNavSettings.Pattern, new { Controller = "Home", Action = "HistoryNav" });//"nav/{**all}"
                            }
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

                //Case aware physical file provider
                CaseAwarePhysicalFileProvider caseAwareContentRootPath = new CaseAwarePhysicalFileProvider(env.ContentRootPath);
                caseAwareContentRootPath.CaseSensitive = true;
                CompositeFileProvider compositeProvider = new CompositeFileProvider(env.ContentRootFileProvider, caseAwareContentRootPath);
                env.ContentRootFileProvider = compositeProvider;

                CaseAwarePhysicalFileProvider caseAwareWebRootPath = new CaseAwarePhysicalFileProvider(env.WebRootPath);
                caseAwareWebRootPath.CaseSensitive = true;
                compositeProvider = new CompositeFileProvider(env.WebRootFileProvider, caseAwareWebRootPath);
                env.WebRootFileProvider = compositeProvider;
                //Case aware physical file provider
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

        public static void AttachWatcher(string dir, string fileName, IHostApplicationLifetime applicationLifetime, Action<bool> restart, UnhandledExceptionEventHandler appDomain_OnUnhandledException,
            ResolveEventHandler appDomain_OnAssemblyResolve)
        {
            if (fileName == null || !File.Exists(Path.Combine(dir, fileName)))
            {
                //Do nothing for none existant folders
                return;
            }
            KraftLogger.LogInformation($"AttachWatcher: Attach watcher {fileName}");
            FileSystemWatcher fileWatcher;
            RestartReason restartReason = new RestartReason();
            fileWatcher = new FileSystemWatcher(dir)
            {
                // watch for everything
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                IncludeSubdirectories = false,
                Filter = fileName,
                InternalBufferSize = 16384
            };
            // Add event handlers.
            fileWatcher.Changed += OnChanged;
            fileWatcher.Created += OnChanged;
            fileWatcher.Deleted += OnChanged;
            fileWatcher.Renamed += OnRenamed;
            fileWatcher.Error += OnError;

            // Begin watching...
            fileWatcher.EnableRaisingEvents = true;

            void OnChanged(object source, FileSystemEventArgs e)
            {
                fileWatcher.EnableRaisingEvents = false;
                //Bug in Docker which will trigger OnChanged during StartUp (How to fix?)
                AppDomain.CurrentDomain.UnhandledException -= appDomain_OnUnhandledException;
                AppDomain.CurrentDomain.AssemblyResolve -= appDomain_OnAssemblyResolve;
                restartReason.Reason = "File Changed";
                restartReason.Description = $"ChangeType: {e.ChangeType} file {e.FullPath}";
                RestartApplication(applicationLifetime, restartReason, restart);
            }

            void OnRenamed(object source, RenamedEventArgs e)
            {
                fileWatcher.EnableRaisingEvents = false;
                AppDomain.CurrentDomain.UnhandledException -= appDomain_OnUnhandledException;
                AppDomain.CurrentDomain.AssemblyResolve -= appDomain_OnAssemblyResolve;
                restartReason.Reason = "File Renamed";
                restartReason.Description = $"Renaming from {e.OldFullPath} to {e.FullPath}";
                RestartApplication(applicationLifetime, restartReason, restart);
            }

            void OnError(object sender, ErrorEventArgs e)
            {
                KraftLogger.LogCritical(e.GetException(), "OnError in AttachWatcher");
            }
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
            //TODO Check why Microsoft.Data.Sqlite, Version=5.0.5.0 requests windows.dll
            if (fileName.Equals("windows.dll", StringComparison.OrdinalIgnoreCase) || fileName.Equals("Microsoft.Windows.SDK.NET.dll", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return Utilities.Generic.Utilities.LoadAssembly(
                    _KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders,
                    "_PluginsReferences",
                    fileName, args.RequestingAssembly.FullName);
        }
        #region Small helpers
        public static string CombinedMessageFromStatusResults(this IReturnStatus status)
        {
            if (status != null && status.StatusResults != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var sr in status.StatusResults)
                {
                    if (sr != null && sr.Message != null)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append(sr.Message);
                    }
                }
                return sb.ToString();
            }
            return null;
        }

        private static void RegisterServicesFromAssembly(IServiceCollection services, Assembly assembly)
        {
            Type initializerType = assembly.GetTypes()
            .FirstOrDefault(t => typeof(IWebApiInitializer).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (initializerType != null)
            {
                object initializerInstance = Activator.CreateInstance(initializerType);

                // Get the InitializeServices method info
                MethodInfo initializeServicesMethod = initializerType.GetMethod("InitializeServices");
                initializeServicesMethod.Invoke(initializerInstance, new object[] { services, true });
            }
        }
        private static void RegisterBuilderFromAssembly(IApplicationBuilder app, Assembly assembly)
        {
            Type initializerType = assembly.GetTypes()
            .FirstOrDefault(t => typeof(IWebApiInitializer).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (initializerType != null)
            {
                object initializerInstance = Activator.CreateInstance(initializerType);

                // Get the InitializeBuilder method info
                MethodInfo initializeBuilderMethod = initializerType.GetMethod("InitializeBuilder");
                initializeBuilderMethod.Invoke(initializerInstance, new object[] { app, true });
            }
        }

        #endregion
    }
}

