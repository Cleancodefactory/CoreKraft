<<<<<<< HEAD
﻿using Ccf.Ck.Libs.Logging;
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
        private Timer _Timer;
        private readonly IServiceScopeFactory _ScopeFactory;

        public SignalService(IServiceScopeFactory scopeFactory)
        {
            _ScopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            KraftLogger.LogInformation("CoreKraft-Background-Service is starting.");
            using (IServiceScope scope = _ScopeFactory.CreateScope())
            {
                _KraftGlobalConfigurationSettings = scope.ServiceProvider.GetRequiredService<KraftGlobalConfigurationSettings>();
                _ServiceProvider = scope.ServiceProvider;
                int minutes = _KraftGlobalConfigurationSettings.GeneralSettings.HostingServiceSettings.Interval;
                if (minutes > 0)
                {
                    _Timer = new Timer(DoWork, _ScopeFactory, TimeSpan.Zero, TimeSpan.FromMinutes(minutes));
                }
            }

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            IServiceScopeFactory scopeFactory = state as IServiceScopeFactory;
            if (scopeFactory != null)
            {
                using (IServiceScope scope = scopeFactory.CreateScope())
                {
                    _KraftGlobalConfigurationSettings = scope.ServiceProvider.GetRequiredService<KraftGlobalConfigurationSettings>();
                    _ServiceProvider = scope.ServiceProvider;
                    foreach (string signal in _KraftGlobalConfigurationSettings.GeneralSettings.HostingServiceSettings.Signals ?? new List<string>())
                    {
                        Stopwatch stopWatch = Stopwatch.StartNew();
                        ExecuteSignals("null", signal);
                        KraftLogger.LogInformation($"Executing signal: {signal} for {stopWatch.ElapsedMilliseconds} ms");
                    }
                }
            }
            KraftLogger.LogInformation("CoreKraft-Background-Service executed.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            KraftLogger.LogInformation("CoreKraft-Background-Service is stopped.");
            _Timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _Timer?.Dispose();
        }
    }
}
=======
﻿using Ccf.Ck.Libs.Logging;
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
        private Timer _Timer;
        private readonly IServiceScopeFactory _ScopeFactory;

        public SignalService(IServiceScopeFactory scopeFactory)
        {
            _ScopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            KraftLogger.LogInformation("CoreKraft-Background-Service is starting.");
            using (IServiceScope scope = _ScopeFactory.CreateScope())
            {
                _KraftGlobalConfigurationSettings = scope.ServiceProvider.GetRequiredService<KraftGlobalConfigurationSettings>();
                _ServiceProvider = scope.ServiceProvider;
                int minutes = _KraftGlobalConfigurationSettings.GeneralSettings.HostingServiceSettings.Interval;
                if (minutes > 0)
                {
                    _Timer = new Timer(DoWork, _ScopeFactory, TimeSpan.Zero, TimeSpan.FromMinutes(minutes));
                }
            }

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            IServiceScopeFactory scopeFactory = state as IServiceScopeFactory;
            if (scopeFactory != null)
            {
                using (IServiceScope scope = scopeFactory.CreateScope())
                {
                    _KraftGlobalConfigurationSettings = scope.ServiceProvider.GetRequiredService<KraftGlobalConfigurationSettings>();
                    _ServiceProvider = scope.ServiceProvider;
                    foreach (string signal in _KraftGlobalConfigurationSettings.GeneralSettings.HostingServiceSettings.Signals ?? new List<string>())
                    {
                        Stopwatch stopWatch = Stopwatch.StartNew();
                        ExecuteSignals("null", signal);
                        KraftLogger.LogInformation($"Executing signal: {signal} for {stopWatch.ElapsedMilliseconds} ms");
                    }
                }
            }
            KraftLogger.LogInformation("CoreKraft-Background-Service executed.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            KraftLogger.LogInformation("CoreKraft-Background-Service is stopped.");
            _Timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _Timer?.Dispose();
        }
    }
}
>>>>>>> develop
