//using Ccf.Ck.Libs.Logging;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Server.Kestrel.Core;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.IO;
//using System.Reflection;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Ccf.Ck.Launchers.Main
//{
//    public class Program
//    {
//        private static string[] _Args;
//        private static bool _RestartRequest;
//        private static CancellationTokenSource _CancellationToken = new CancellationTokenSource();

//        public static async Task Main(string[] args)
//        {
//            _Args = args;
//            //_Args = new string[1];
//            //_Args[0] = @"/mnt/d/_Development/Ccf/CcfRepositories/Customers/Website_AndGrains";

//            await StartServer();
//            while (_RestartRequest)
//            {
//                _RestartRequest = false;
//                await StartServer();
//            }
//        }

//        public static void Restart(bool restart)
//        {
//            _RestartRequest = restart;
//            if (restart)
//            {
//                KraftLogger.LogInformation($"Restart: executed");
//                Console.WriteLine($"========= Restarting App at: {DateTime.Now.ToLongTimeString()} =========");
//            }
//            _CancellationToken.Cancel();
//        }

//        private static async Task StartServer()
//        {
//            try
//            {
//                _CancellationToken = new CancellationTokenSource();
//                IHostBuilder hostBuilder = CreateHostBuilder(_Args);
//                await hostBuilder.RunConsoleAsync(_CancellationToken.Token);                
//            }
//            catch (OperationCanceledException e)
//            {
//                Console.WriteLine(e);
//            }
//        }

//        public static IHostBuilder CreateHostBuilder(string[] args)
//        {
//            string contentRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
//            if (args.Length > 0)
//            {
//                contentRoot = Path.GetFullPath(args[0]);
//            }
//            Console.WriteLine("Content root: " + contentRoot);
//            return Host.CreateDefaultBuilder(args)
//                .UseContentRoot(contentRoot)
//                .ConfigureLogging((hostBuilder, logging) =>
//                {
//                    logging.ClearProviders();
//                })
//                .ConfigureWebHostDefaults(webBuilder =>
//                {
//                    webBuilder.UseContentRoot(contentRoot);
//                    webBuilder.ConfigureKestrel(serverOptions =>
//                    {
//                        serverOptions.Limits.MaxConcurrentConnections = null;
//                        serverOptions.Limits.MaxConcurrentUpgradedConnections = null;
//                        serverOptions.Limits.MaxRequestBodySize = null;
//                        serverOptions.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(20));
//                        serverOptions.Limits.MinResponseDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(20));
//                    });
//                    webBuilder.UseStartup<Startup>();
//                });
//        }
//    }
//}
