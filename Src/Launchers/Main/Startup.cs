using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ccf.Ck.Web.Middleware;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace Ccf.Ck.Launchers.Main
{
    public class Startup
    {
        private IConfigurationRoot _Configuration { get; }

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
            services.UseBindKraft(_Configuration);
            services.AddMvc();
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
        
    }
}
