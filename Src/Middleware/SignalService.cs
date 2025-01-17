using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Ccf.Ck.Utilities.Generic.Utilities;

namespace Ccf.Ck.Web.Middleware
{
    internal class SignalService : SignalBase, IHostedService, IDisposable
    {
        private readonly List<Timer> _Timers;
        private readonly IServiceScopeFactory _ScopeFactory;
        private List<FileSystemWatcher> _FileWatchers;
        static readonly object _Lock = new object();

        public SignalService(IServiceScopeFactory scopeFactory)
        {
            _ScopeFactory = scopeFactory;
            _Timers = new List<Timer>();
            _FileWatchers = new List<FileSystemWatcher>(100);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            KraftLogger.LogInformation("SignalService: StartAsync executed.");
            _ServiceProvider = _ScopeFactory.CreateScope().ServiceProvider;
            _KraftGlobalConfigurationSettings = _ServiceProvider.GetRequiredService<KraftGlobalConfigurationSettings>();

            foreach (HostingServiceSetting item in _KraftGlobalConfigurationSettings.GeneralSettings.HostingServiceSettings)
            {
                int minutes = item.IntervalInMinutes;
                if (minutes > 0)
                {
                    TimeSpan initialDelay = CalculateInitialDelay(item.ActiveDays, item.StartTime);
                    Timer t = new Timer(DoWork, item.Signals, initialDelay, TimeSpan.FromMinutes(item.IntervalInMinutes));
                    _Timers.Add(t);
                }
            }
            return Task.CompletedTask;
        }

        private TimeSpan CalculateInitialDelay(List<DayOfWeek> activeDays, TimeSpan startTime)
        {
            DateTime now = DateTime.Now;
            if (activeDays == null || activeDays.Count == 0)
            {
                // If no specific days are configured, assume every day.
                activeDays = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList();
            }

            DateTime nextRun = now.Date + startTime;

            if (nextRun < now || !activeDays.Contains(now.DayOfWeek))
            {
                // Find the next valid day
                for (int i = 1; i <= 7; i++)
                {
                    DateTime potentialNextRun = now.AddDays(i).Date + startTime;
                    if (activeDays.Contains(potentialNextRun.DayOfWeek))
                    {
                        nextRun = potentialNextRun;
                        break;
                    }
                }
            }

            return nextRun - now;
        }

        private void DoWork(object state)
        {
            lock (_Lock)
            {
                if (state is List<string> signals)
                {
                    DateTime now = DateTime.Now;
                    HostingServiceSetting setting = _KraftGlobalConfigurationSettings
                        .GeneralSettings
                        .HostingServiceSettings
                        .FirstOrDefault(s => s.Signals.SequenceEqual(signals));

                    if (setting != null &&
                        setting.ActiveDays.Contains(now.DayOfWeek) &&
                        now.TimeOfDay >= setting.StartTime &&
                        now.TimeOfDay < setting.StartTime.Add(TimeSpan.FromMinutes(setting.IntervalInMinutes)))
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
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            KraftLogger.LogInformation("SignalService: StopAsync executed");
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
            foreach (FileSystemWatcher fileSystemWatcher in _FileWatchers)
            {
                fileSystemWatcher.EnableRaisingEvents = false;
                fileSystemWatcher.Dispose();
            }
        }

        public void AttachModulesWatcher(string moduleFolder, 
            bool includeSubdirectories, 
            IHostApplicationLifetime applicationLifetime, 
            Action<bool> restart,
            UnhandledExceptionEventHandler appDomain_OnUnhandledException, 
            ResolveEventHandler appDomain_OnAssemblyResolve)
        {
            if (!Directory.Exists(moduleFolder))
            {
                //Do nothing for none existant folders
                return;
            }
            FileSystemWatcher fileWatcher;
            RestartReason restartReason = new RestartReason();
            fileWatcher = new FileSystemWatcher(moduleFolder)
            {
                // watch for everything
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = includeSubdirectories,
                Filter = "*.*",
                InternalBufferSize = 16384
            };
            // Add event handlers.
            fileWatcher.Changed += OnChanged;
            fileWatcher.Created += OnChanged;
            fileWatcher.Deleted += OnChanged;
            fileWatcher.Renamed += OnRenamed;
            fileWatcher.Error += OnError;

            // Begin watching...
            fileWatcher.EnableRaisingEvents = true;
            _FileWatchers.Add(fileWatcher);

            void OnChanged(object source, FileSystemEventArgs e)
            {
                fileWatcher.EnableRaisingEvents = false;
                //Bug in Docker which will trigger OnChanged during StartUp (How to fix?)
                AppDomain.CurrentDomain.UnhandledException -= appDomain_OnUnhandledException;
                AppDomain.CurrentDomain.AssemblyResolve -= appDomain_OnAssemblyResolve;
                restartReason.Reason = "File Changed";
                restartReason.Description = $"ChangeType: {e.ChangeType} file {e.FullPath}";
                RestartApplication(applicationLifetime, restartReason, restart);
            }

            void OnRenamed(object source, RenamedEventArgs e)
            {
                fileWatcher.EnableRaisingEvents = false;
                AppDomain.CurrentDomain.UnhandledException -= appDomain_OnUnhandledException;
                AppDomain.CurrentDomain.AssemblyResolve -= appDomain_OnAssemblyResolve;
                restartReason.Reason = "File Renamed";
                restartReason.Description = $"Renaming from {e.OldFullPath} to {e.FullPath}";
                RestartApplication(applicationLifetime, restartReason, restart);
            }

            void OnError(object sender, ErrorEventArgs e)
            {
                KraftLogger.LogCritical(e.GetException(), "OnError in AttachModulesWatcher");
            }
        }
    }
}
