using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ccf.Ck.Web.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.IO;
using System.Reflection;
using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Http;
using Ccf.Ck.Launchers.Main.Routing;

namespace Ccf.Ck.Launchers.Main
{
    public class Startup
    {
        private IConfigurationRoot _Configuration { get; }
        private KraftGlobalConfigurationSettings _KraftGlobalConfiguration;

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
            if (_KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.IsConfigured)
            {
                services.AddMvc().ConfigureApplicationPartManager(ConfigureApplicationParts);
                services.AddSingleton<DynamicHostRouteTransformer>();
            }
            else
            {
                services.AddMvc();
            }

            //services.AddOptions();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseBindKraft(env);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();
            if (_KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.IsConfigured)
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapDynamicControllerRoute<DynamicHostRouteTransformer>(_KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.DefaultRouting);
                });
            }
            else
            {
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                });
            }
            //ChangeToken.OnChange(
            //    () => _Configuration.GetReloadToken(),
            //    (state) => InvokeChanged(state),
            //    env);
        }

        private void ConfigureApplicationParts(ApplicationPartManager apm)
        {
            foreach (string rootFolder in _KraftGlobalConfiguration.GeneralSettings.ModulesRootFolders)
            {
                string rootPath = Path.Combine(rootFolder, "_PluginsReferences");
                FileInfo assemblyFile = new FileInfo(Path.Combine(rootPath, _KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.AssemblyNameCode));
                if (assemblyFile.Exists)
                {
                    Assembly assemblyCode = Assembly.LoadFile(assemblyFile.FullName);
                    apm.ApplicationParts.Add(new AssemblyPart(assemblyCode));
                    assemblyFile = new FileInfo(Path.Combine(rootPath, _KraftGlobalConfiguration.GeneralSettings.RazorAreaAssembly.AssemblyNameViews));
                    if (assemblyFile.Exists)
                    {
                        Assembly assemblyViews = Assembly.LoadFile(assemblyFile.FullName);
                        apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(assemblyViews));
                    }
                }
            }
        }
    }
}
