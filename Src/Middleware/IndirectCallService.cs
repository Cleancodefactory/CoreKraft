﻿using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ccf.Ck.Web.Middleware
{
    public class IndirectCallService : IIndirectCallService, IHostedService, IIndirectCallerControl {
        // This is singleton so when updated from the config nothing is done with them yet
        protected static int TIMEOUT_SECONDS = 120;
        protected static int RESULT_PRESERVE_SECONDS = 3600;
        protected static int SCHEDULE_TIMEOUT_SECONDS = 84000;
        protected static int WORKER_THREADS = 2;
        protected static int STARTUP_DELAY = 10;

        private bool _started = false;

        KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings = null;

        /// <summary>
        /// Contains all the tasks which are not yet executed
        /// </summary>
        private Queue<QueuedTask> _Waiting = new Queue<QueuedTask>();
        /// <summary>
        /// Despite its name contains the running and finished tasks
        /// </summary>
        private Dictionary<Guid, TaskHolder> _Finished = new Dictionary<Guid, TaskHolder>();

        private AutoResetEvent _ThreadSignal = new AutoResetEvent(true);

        private Thread _SchedulerThread;
        
        private bool _Continue = true;
        // TODO: With multiple threads we will need synchronization, put the neccessary stuff here
        private object _lockObject = new object();

        private IServiceScopeFactory _ScopeFactory;

        #region Construction
        public IndirectCallService(IServiceScopeFactory scopeFactory, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) {
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            _ScopeFactory = scopeFactory;
            _SchedulerThread = new Thread(new ThreadStart(this.Scheduler));
            _SchedulerThread.IsBackground = true;
            // Update from configuration
            
        }
        private void _UpdateFromConfiguration(KraftGlobalConfigurationSettings global) {
            if (global != null) {
                var scheduler = global.CallScheduler;
                if (scheduler != null) {
                    if (scheduler.StartupDelay >= 0) STARTUP_DELAY = scheduler.StartupDelay;
                    if (scheduler.RecheckSeconds >=0) TIMEOUT_SECONDS = scheduler.RecheckSeconds;
                    if (scheduler.ResultPreserveSeconds >= 0) RESULT_PRESERVE_SECONDS = scheduler.ResultPreserveSeconds;
                    if (scheduler.ScheduleTimeoutSeconds >= 0) SCHEDULE_TIMEOUT_SECONDS = scheduler.ScheduleTimeoutSeconds;
                    if (scheduler.WorkerThreads >= 1) WORKER_THREADS = scheduler.WorkerThreads;
                }
            }
        }
        #endregion Construction

        #region Scheduler
        public Task StartAsync(CancellationToken cancellationToken) {
            KraftLogger.LogInformation("IndirectCallService: StartAsync executed.");
            _SchedulerThread.Start();
            return Task.FromResult(0);
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            _Continue = false;
            _ThreadSignal.Set();
            KraftLogger.LogInformation("IndirectCallService:StopAsync executed");
            _SchedulerThread.Join();
            return Task.FromResult(0);
        }
        private void Scheduler() {
            var _timeout_seconds = STARTUP_DELAY; // 
            while (_Continue) {
                if (_started) {
                    QueuedTask waiting = null;
                    lock (_Waiting) {
                        if (!_Waiting.TryDequeue(out waiting)) {
                            waiting = null;
                        };
                    }
                    if (waiting != null) {
                        // Discard timeouted tasks
                        if (DateTime.Now - waiting.queued > TimeSpan.FromSeconds(waiting.scheduleTimeout)) {
                            // Move this to finished
                            waiting.task.status = IndirectCallStatus.Discarded;
                            lock (_Finished) {
                                _Finished.Add(waiting.task.guid, waiting.task);
                            }
                            continue;
                        }
                        var result = waiting.task;
                        result.started = DateTime.Now;
                        result.status = IndirectCallStatus.Running;
                        lock (_Finished) {
                            _Finished.Add(result.guid, result);
                        }
                        // Mark call as indirect call
                        if (result.input != null) result.input.CallType = Models.Enumerations.ECallType.ServiceCall;
                        // We depend on the DirectCall to indicate the success in the ReturnModel
                        var returnModel = DirectCallService.Instance.Call(result.input);
                        result.result = returnModel;
                        result.finished = DateTime.Now;
                        result.status = IndirectCallStatus.Finished;
                        continue; // Check for more tasks before waiting
                    } else {
                        ScheduleOnEmptyQueue();
                    }
                }
                _ThreadSignal.WaitOne(TimeSpan.FromSeconds(_timeout_seconds));
                lock (_lockObject) {
                    _started = true;
                }
                _timeout_seconds = TIMEOUT_SECONDS;
                // Clean up old results
                CleanUpResults();
            }
        }
        private void CleanUpResults() {
            lock (_Finished) {
                List<Guid> _toclean = new List<Guid>(_Finished.Count / 2);
                foreach (var kv in _Finished) {
                    var tsk = kv.Value;
                    if (tsk.finished.HasValue &&
                        tsk.status == IndirectCallStatus.Finished && 
                        (DateTime.Now - tsk.finished) > TimeSpan.FromSeconds(RESULT_PRESERVE_SECONDS)) {
                        _toclean.Add(kv.Key);
                    }
                }
                _toclean.ForEach(k => _Finished.Remove(k));
            }
        }


        #endregion Scheduler

        #region Callback helpers
        private void ScheduleOnEmptyQueue() {
            var handler_def = _KraftGlobalConfigurationSettings?.CallScheduler?.OnEmptyQueue;
            if (handler_def != null) {
                InputModel im = new InputModel();
                if (!im.ParseAddress(handler_def.Address)) {
                    throw new Exception($"Cannot parse address {handler_def.Address}");
                }
                im.IsWriteOperation = handler_def.IsWriteOperation;
                im.RunAs = handler_def.RunAs;
                im.Data = new Dictionary<string, object>() {
                    { "scheduler", this}
                };
                this.Call(im);

            }
        }
        #endregion


        #region Types

        private record TaskHolder(
            Guid guid, 
            InputModel input, 
            ReturnModel result, 
            IndirectCallStatus status,
            DateTime? started, 
            DateTime? finished) {

            public IndirectCallStatus status { get; set; } = status;
            public ReturnModel result { get; set; } = result;
            public DateTime? started { get; set; } = started;
            public DateTime? finished { get; set; } = finished;
        }

        private record QueuedTask(TaskHolder task, DateTime queued, int scheduleTimeout) {
            public int scheduleTimeout { get; init; } = scheduleTimeout > 0? scheduleTimeout:SCHEDULE_TIMEOUT_SECONDS;
        }

        

        #endregion Types

        #region Public access

        public Guid Call(InputModel input, int timeout = 84000)
        {
            timeout = SCHEDULE_TIMEOUT_SECONDS;
            if (input == null) return Guid.Empty;
            var tsk = new TaskHolder(Guid.NewGuid(), input, null, IndirectCallStatus.Queued, null, null);
            var waiting = new QueuedTask(tsk,DateTime.Now,timeout != 0?timeout: SCHEDULE_TIMEOUT_SECONDS);
            lock(_Waiting)
            {
                _Waiting.Enqueue(waiting);
            }
            _ThreadSignal.Set();
            return waiting.task.guid;
        }
        public IndirectCallStatus CallStatus(Guid guid)
        {
            TaskHolder task = null;
            lock(_Finished)
            {
                if (_Finished.TryGetValue(guid, out task)) {
                    return task.status;
                } else {
                    lock (_Waiting) {
                        if (_Waiting.Any(t => t.task.guid == guid)) {
                            return IndirectCallStatus.Queued;
                        } else {
                            return IndirectCallStatus.Unavailable;
                        }
                    }
                }
            }
        }
        public ReturnModel GetResult(Guid guid) {
            TaskHolder task = null;
            lock (_Finished) {
                if (_Finished.TryGetValue(guid, out task)) {
                    return task.result;
                }
            }
            return null;
        }

        public IIndirectCallInfo GetIndirectCallInfo(Guid guid) {
            TaskHolder task = null;
            lock (_Finished) {
                if (_Finished.TryGetValue(guid, out task)) {
                    return new IndirectCallInfo(guid, task.status, task.result, task.started, task.finished);
                } else {
                    lock (_Waiting) {
                        if (_Waiting.Any(t => t.task.guid == guid)) {
                            return new IndirectCallInfo(guid, IndirectCallStatus.Queued);
                        } else {
                            return new IndirectCallInfo(guid, IndirectCallStatus.Unavailable);
                        }
                    }
                }
            }
        }
        #endregion

        #region Control interface

        public IIndirectCallerInfo GetIndirectServiceInfo() {
            var waiting = _Waiting.Select(qt => new IndirectCallInfoEx(qt.task.guid,
                                                        IndirectCallStatus.Queued,
                                                        qt.task.input,
                                                        qt.task.result,
                                                        null,
                                                        null,
                                                        qt.queued));
            var finished = _Finished.Select(kv => new IndirectCallInfoEx(
                kv.Value.guid,
                kv.Value.status,
                kv.Value.input,
                kv.Value.result,
                kv.Value.started,
                kv.Value.finished
            ));
            return new IndirectCallerInfo(waiting, finished);

        }

        #endregion



    }


}
