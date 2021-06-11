using Ccf.Ck.Launchers.Main.ActionFilters;
using Ccf.Ck.Launchers.Main.Routing;
using Ccf.Ck.Launchers.Main.Utils;
using Ccf.Ck.Models.EmailSettings;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;

namespace Ccf.Ck.Launchers.Main
{
    public class Startup
    {
        private IConfigurationRoot _Configuration { get; }
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
            IServiceProvider serviceProvider = services.UseBindKraft(_Configuration);
            _KraftGlobalConfiguration = serviceProvider.GetService<KraftGlobalConfigurationSettings>();
            EmailSettings emailSettings = new EmailSettings();
            _Configuration.GetSection("EmailSettings").Bind(emailSettings);
            services.AddSingleton(emailSettings);
            if (_KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.IsConfigured)
            {
                services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential 
                    // cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    // requires using Microsoft.AspNetCore.Http;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });
                services.AddControllersWithViews(options =>
                {
                    options.Filters.Add(typeof(CultureActionFilter));
                }).ConfigureApplicationPartManager(ConfigureApplicationParts).AddTagHelpersAsServices();
                services.AddSingleton<DynamicHostRouteTransformer>();
            }
            else
            {
                services.AddControllersWithViews(options =>
                {
                    options.Filters.Add(typeof(CultureActionFilter));
                });
            }
            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            app.UseBindKraft(env, Program.Restart);
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

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                if (_KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.IsConfigured)
                {
                    endpoints.MapDynamicControllerRoute<DynamicHostRouteTransformer>(_KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.DefaultRouting);
                }
                endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                name: "catchall",
                pattern: "/{**catchAll}", new { Controller = "Home", Action = "CatchAll" });
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
                GC.Collect();
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
                    }
                }
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
