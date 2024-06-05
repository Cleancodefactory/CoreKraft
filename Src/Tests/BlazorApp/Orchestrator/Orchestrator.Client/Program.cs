using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using Orchestrator.Client.Services;

namespace Orchestrator.Client
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddScoped(sp => new HttpClient());
            builder.Services.AddScoped<MapService>();
            builder.Services.AddFluentUIComponents();

            await builder.Build().RunAsync();
        }
    }
}
