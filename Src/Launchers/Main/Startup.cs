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
            services.AddMvc();
            //_KraftGlobalConfiguration = serviceProvider.GetService<KraftGlobalConfigurationSettings>();
            //services.AddMvc().ConfigureApplicationPartManager(ConfigureApplicationParts);
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
            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            //ChangeToken.OnChange(
            //    () => _Configuration.GetReloadToken(),
            //    (state) => InvokeChanged(state),
            //    env);
        }

        private void ConfigureApplicationParts(ApplicationPartManager apm)
        {
            var rootPath = Path.Combine(_KraftGlobalConfiguration.GeneralSettings.DefaultStartModule, "_PluginsReferences");

            var assemblyFiles = Directory.GetFiles(rootPath, "*.dll");
            foreach (string assemblyFile in assemblyFiles)
            {
                try
                {
                    if (assemblyFile.Contains("Ccf.Ck.LandingPage.Tcd", StringComparison.OrdinalIgnoreCase))
                    {
                        var assembly = Assembly.LoadFile(assemblyFile);
                        if (assemblyFile.EndsWith(this.GetType().Namespace + ".Views.dll") || assemblyFile.EndsWith(this.GetType().Namespace + ".dll"))
                            continue;
                        else if (assemblyFile.EndsWith(".Views.dll"))
                            apm.ApplicationParts.Add(new CompiledRazorAssemblyPart(assembly));
                        else
                            apm.ApplicationParts.Add(new AssemblyPart(assembly));
                    }
                }
                catch (Exception e) { }
            }
        }

    }
}
