using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Ccf.Ck.Launchers.Main
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHost webHost = BuildWebHost(args);
            webHost.Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            string contentRoot = Directory.GetCurrentDirectory();
            if (args.Length > 0)
            {
                contentRoot = args[0];
            }
            return WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(contentRoot)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .PreferHostingUrls(true)
                .Build();
        }
    }
}
