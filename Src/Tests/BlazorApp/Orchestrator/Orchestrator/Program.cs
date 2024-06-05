using Ccf.Ck.Web.Middleware;
using Microsoft.FluentUI.AspNetCore.Components;
using Orchestrator.Client.Services;
using Orchestrator.Components;

namespace Orchestrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();
            builder.Services.AddFluentUIComponents();

            builder.Services.AddMvc(opt =>
            {
                opt.EnableEndpointRouting = false;
            });

            builder.Services.AddScoped(sp => new HttpClient());

            builder.Services.AddScoped<MapService>();

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            IConfiguration confg = configurationBuilder.Build();
            builder.Services.UseBindKraft(confg);

            var app = builder.Build();
            app.UseBindKraft(app.Environment);

            app.UseMvcWithDefaultRoute();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
