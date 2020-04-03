using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Ccf.Ck.Web.Middleware
{
    internal class SignalService : SignalBase, IHostedService, IDisposable
    {
        private readonly List<Timer> _Timers;
        private readonly IServiceScopeFactory _ScopeFactory;
        static readonly object _Lock = new object();

        public SignalService(IServiceScopeFactory scopeFactory)
        {
            _ScopeFactory = scopeFactory;
            _Timers = new List<Timer>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            KraftLogger.LogInformation("CoreKraft-Background-Service is starting.");
            using (IServiceScope scope = _ScopeFactory.CreateScope())
            {
                _KraftGlobalConfigurationSettings = scope.ServiceProvider.GetRequiredService<KraftGlobalConfigurationSettings>();
                _ServiceProvider = scope.ServiceProvider;
                foreach (HostingServiceSetting item in _KraftGlobalConfigurationSettings.GeneralSettings.HostingServiceSettings)
                {
                    int minutes = item.IntervalInMinutes;
                    if (minutes > 0)
                    {
                        Timer t = new Timer(DoWork, item.Signals, TimeSpan.FromMinutes(minutes), TimeSpan.FromMinutes(minutes));
                        _Timers.Add(t);
                    }
                }
                
            }
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            lock(_Lock)
            {
                List<string> signals = state as List<string>;
                if (signals != null)
                {
                    foreach (string signal in signals)
                    {
                        Stopwatch stopWatch = Stopwatch.StartNew();
                        ExecuteSignals("null", signal);
                        KraftLogger.LogInformation($"Executing signal: {signal} for {stopWatch.ElapsedMilliseconds} ms");
                    }
                    KraftLogger.LogInformation("Batch of CoreKraft-Background-Services executed.");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            KraftLogger.LogInformation("CoreKraft-Background-Service is stopped.");
            foreach (Timer timer in _Timers)
            {
                timer?.Change(Timeout.Infinite, 0);
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            foreach (Timer timer in _Timers)
            {
                timer?.Dispose();
            }            
        }
    }
}
