using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Ccf.Ck.Launcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHost webHost = BuildWebHost(args);
            webHost.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .PreferHostingUrls(true)
                .Build();
    }
}
