using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Ccf.Ck.Launchers.Main
{
    public class Program
    {
        private static string[] _Args;
        private static bool _RestartRequest;
        private static CancellationTokenSource _CancellationToken = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            _Args = args;

            await StartServer();
            while (_RestartRequest)
            {
                _RestartRequest = false;
                Console.WriteLine("Restarting App");
                await StartServer();
            }
        }

        public static void Restart(bool restart)
        {
            _RestartRequest = restart;
            _CancellationToken.Cancel();
        }

        private static async Task StartServer()
        {
            try
            {
                _CancellationToken = new CancellationTokenSource();
                await BuildWebHost(_Args).RunAsync(_CancellationToken.Token);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e);
            }
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            string contentRoot = Directory.GetCurrentDirectory();
            if (args.Length > 0)
            {
                contentRoot = Path.GetFullPath(args[0]);
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
