using Ccf.Ck.Launchers.Main.ActionFilters;
using Ccf.Ck.Launchers.Main.Routing;
using Ccf.Ck.Launchers.Main.Utils;
using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Libs.SendEmailExtended;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Web.Middleware;
using Ccf.Ck.Web.Middleware.Aws;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ccf.Ck.Launchers.Main
{
    public class Startup
    {
        private IConfigurationRoot _Configuration { get; set; }
        private KraftGlobalConfigurationSettings _KraftGlobalConfiguration;
        private static RazorAssemblyLoadContext _RazorAssemblyLoadContext;
        private static ApplicationPartManager _ApplicationPartManager;

        public Startup(IWebHostEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            _Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            AwsConfiguration awsConfiguration = new AwsConfiguration();
            _Configuration.GetSection("AwsConfiguration").Bind(awsConfiguration);
            if (awsConfiguration != null && !string.IsNullOrEmpty(awsConfiguration.Name) && !string.IsNullOrEmpty(awsConfiguration.Region))
            {
                string secretName = awsConfiguration.Name;
                string region = awsConfiguration.Region;
                IConfigurationBuilder builder = new ConfigurationBuilder();
                AmazonSecretsManagerConfigurationSource configurationSource = new AmazonSecretsManagerConfigurationSource(region, secretName);
                builder.Add(configurationSource);
                _Configuration = builder.Build();
            }

            IServiceProvider serviceProvider = services.UseBindKraft(_Configuration);
            _KraftGlobalConfiguration = serviceProvider.GetService<KraftGlobalConfigurationSettings>();
            EmailSettings emailSettings = new EmailSettings();
            _Configuration.GetSection("EmailSettings").Bind(emailSettings);
            services.AddSingleton(emailSettings);
            if (_KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.IsConfigured 
                && _KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.IsEnabled)
            {
                ConfigureCookiePolicy(services);
                services.AddControllersWithViews(options =>
                {
                    options.Filters.Add(typeof(CultureActionFilter));
                }).ConfigureApplicationPartManager(ConfigureApplicationParts).AddTagHelpersAsServices();
                services.AddSingleton<DynamicHostRouteTransformer>();
            }
            else if (_KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.IsConfigured
                && _KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.IsEnabled)
            {
                ConfigureCookiePolicy(services);
                services.AddMudServices();
                services.AddRazorComponents().AddInteractiveServerComponents();
                services.AddControllersWithViews(options =>
                {
                    options.Filters.Add(typeof(CultureActionFilter));
                });
            }
            else
            {
                services.AddControllersWithViews(options =>
                {
                    options.Filters.Add(typeof(CultureActionFilter));
                });
            }
            services.AddHttpClient();

            // TODO enable through configuration
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });
        }

        private static void ConfigureCookiePolicy(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.Secure = CookieSecurePolicy.Always;
                // This lambda determines whether user consent for non-essential 
                // cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                // requires using Microsoft.AspNetCore.Http;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            app.UseBindKraft(env, Program.Restart);
            //Enable only for debug
            //app.Use(async (context, next) =>
            //{
            //    HttpRequest request = context.Request;
            //    //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //    if (context.User.Identity.IsAuthenticated)
            //    {
            //        string s = "GET";
            //    }
            //    string redirectUrl = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), request.PathBase.ToUriComponent(), request.Path.ToUriComponent(), request.QueryString.ToUriComponent());

            //    // Call the next delegate/middleware in the pipeline.
            //    await next(context);
            //});
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //Called only to show url:port in console during development
                lifetime.ApplicationStarted.Register(() => LogAddresses(app.ServerFeatures, env));
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            app.UseCookiePolicy();
            if (_KraftGlobalConfiguration.GeneralSettings.MiddleWares.Count > 0)
            {
                foreach (MiddleWareSettings middleWare in _KraftGlobalConfiguration.GeneralSettings.MiddleWares)
                {
                    Type ImplementationAsType = Type.GetType(middleWare.ImplementationAsString, true);
                    Type InterfaceAsType = Type.GetType(middleWare.InterfaceAsString);

                    TypeInfo typeInfo = ImplementationAsType.GetTypeInfo();
                    if (typeInfo.ImplementedInterfaces.Contains(InterfaceAsType))
                    {
                        IUseMiddleWare instance = Activator.CreateInstance(ImplementationAsType) as IUseMiddleWare;
                        instance.Use(app);
                    }
                    else
                    {
                        //Error type does not implement interface
                    }
                }
            }
            app.UseRouting();
            app.UseAntiforgery();
            app.UseEndpoints(endpoints =>
            {
                if (_KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.IsConfigured && 
                _KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.IsEnabled)
                {
                    endpoints.MapDynamicControllerRoute<DynamicHostRouteTransformer>(_KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.DefaultRouting);
                    foreach (RouteMapping mapping in _KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.RouteMappings)
                    {
                        if (!string.IsNullOrEmpty(mapping.Pattern))
                        {
                            endpoints.MapControllerRoute(
                            name: mapping.Name,
                            pattern: mapping.Pattern, new { Controller = mapping.Controller, Action = mapping.Action });
                        }
                    }
                }
                else if (_KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.IsConfigured
                                && _KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.IsEnabled)
                {
                    Assembly myAssembly = null;
                    foreach (string rootFolder in _KraftGlobalConfiguration.GeneralSettings.ModulesRootFolders)
                    {
                        string rootPath = Path.Combine(rootFolder, "_PluginsReferences");
                        FileInfo assemblyFile;
                        foreach (string codeAssembly in _KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.BlazorAssemblyNamesCode)
                        {
                            assemblyFile = new FileInfo(Path.Combine(rootPath, codeAssembly));
                            if (assemblyFile.Exists)
                            {
                                myAssembly = Assembly.LoadFile(assemblyFile.FullName);
                            }
                            else
                            {
                                KraftLogger.LogCritical($"Configured assembly {assemblyFile} is missing or error during compilation! No landing page will be loaded.");
                            }
                        }
                    }

                    Type appType = myAssembly.GetType(_KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.BlazorStartApplicationWithNamespace);

                    endpoints.MapStaticAssets();

                    MethodInfo method = typeof(RazorComponentsEndpointRouteBuilderExtensions)
                        .GetMethod("MapRazorComponents", BindingFlags.Static | BindingFlags.Public);
                    if (method != null)
                    {
                        // Create a generic method using your loaded type
                        var genericMethod = method.MakeGenericMethod(appType);
                        // Invoke the generic method. The first parameter is null because it's a static method,
                        // and the second parameter is an array containing the arguments (here, 'app')
                        object cvbuilder = genericMethod.Invoke(null, new object[] { endpoints });
                        RazorComponentsEndpointConventionBuilder convention_builder = cvbuilder as RazorComponentsEndpointConventionBuilder;
                        if (convention_builder != null)
                        {
                            convention_builder.AddInteractiveServerRenderMode();
                        }
                        else
                        {
                            throw new Exception("Convention builder not returned");
                        }
                    }
                    else
                    {
                        throw new Exception("Cannot find MapRazorComponents method");
                    }
                }

                // Controller supporting redirect acceptor pages
                endpoints.MapControllerRoute(
                name: "acceptor",
                pattern: "redirect/{action=Index}/{id?}",
                defaults: new { controller = "Redirect", action = "Index" });

                endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                name: "catchall",
                pattern: "/{**catchAll}", new { Controller = "Home", Action = "CatchAll" });

                //endpoints.MapFallbackToFile("{**angular}", "search-app/index.html");
            });
        }

        private void ConfigureApplicationParts(ApplicationPartManager apm)
        {
            if (_RazorAssemblyLoadContext != null && _ApplicationPartManager != null)
            {
                for (int i = _ApplicationPartManager.ApplicationParts.Count - 1; i >= 0; i--)
                {
                    _ApplicationPartManager.ApplicationParts.Remove(_ApplicationPartManager.ApplicationParts[i]);
                }
                for (int i = _ApplicationPartManager.FeatureProviders.Count - 1; i >= 0; i--)
                {
                    _ApplicationPartManager.FeatureProviders.Remove(_ApplicationPartManager.FeatureProviders[i]);
                }
                Console.WriteLine($"Unloading razor assemblies");
                _RazorAssemblyLoadContext.Unload();
                _RazorAssemblyLoadContext = null;
                _ApplicationPartManager = null;
                GC.WaitForPendingFinalizers();
            }
            _ApplicationPartManager = apm;
            //    //string codeBase = Assembly.GetExecutingAssembly().Location;
            //    //UriBuilder uri = new UriBuilder(codeBase);
            //    //string path = Uri.UnescapeDataString(uri.Path);
            //    //_RazorAssemblyLoadContext = new RazorAssemblyLoadContext(Path.GetDirectoryName(path));
            _RazorAssemblyLoadContext = new RazorAssemblyLoadContext();
            foreach (string rootFolder in _KraftGlobalConfiguration.GeneralSettings.ModulesRootFolders)
            {
                string rootPath = Path.Combine(rootFolder, "_PluginsReferences");
                FileInfo assemblyFile;
                foreach (string codeAssembly in _KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.AssemblyNamesCode)
                {
                    assemblyFile = new FileInfo(Path.Combine(rootPath, codeAssembly));
                    if (assemblyFile.Exists)
                    {
                        //Assembly assemblyCode = Assembly.LoadFile(assemblyFile.FullName);
                        Assembly assemblyCode = _RazorAssemblyLoadContext.LoadFromAssemblyPath(assemblyFile.FullName);
                        apm.ApplicationParts.Add(new AssemblyPart(assemblyCode));
                        // https://docs.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/6.0/razor-compiler-doesnt-produce-views-assembly
                        apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(assemblyCode));
                    }
                    else
                    {
                        KraftLogger.LogCritical($"Configured assembly {assemblyFile} is missing or error during compilation! No landing page will be loaded.");
                    }
                }
                // TODO Remove when all applications are migrated to .NET 6.0
                foreach (string viewAssembly in _KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.AssemblyNamesView)
                {
                    assemblyFile = new FileInfo(Path.Combine(rootPath, viewAssembly));
                    if (assemblyFile.Exists)
                    {
                        //Assembly assemblyViews = Assembly.LoadFile(assemblyFile.FullName);
                        Assembly assemblyViews = _RazorAssemblyLoadContext.LoadFromAssemblyPath(assemblyFile.FullName);
                        apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(assemblyViews));
                    }
                }
            }
        }

        private void LogAddresses(IFeatureCollection features, IWebHostEnvironment env)
        {
            IServerAddressesFeature addressFeature = features.Get<IServerAddressesFeature>();
            foreach (string address in addressFeature.Addresses)
            {
                Console.WriteLine($"Now listening on: {address}");
            }
            Console.WriteLine("Application started. Press Ctrl+C to shut down");
            string environment = "Production";
            if (env.IsDevelopment())
            {
                environment = "Development";
            }
            Console.WriteLine($"Hosting environment: {environment}");
            Console.WriteLine($"Content root path: {env.ContentRootPath}");
        }
    }
}
