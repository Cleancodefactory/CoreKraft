using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ccf.Ck.Web.Middleware
{
    public class IndirectCallService : IIndirectCallService, IHostedService, IIndirectCallerControl
    {
        // This is singleton so when updated from the config nothing is done with them yet
        protected static int TIMEOUT_SECONDS = 120;
        protected static int RESULT_PRESERVE_SECONDS = 3600;
        protected static int SCHEDULE_TIMEOUT_SECONDS = 84000;
        protected static int WORKER_THREADS = 2;
        protected static int STARTUP_DELAY = 30;

        #region Callback constants
        public const string INPUT_MODEL_NAME = "input";
        public const string RETURN_MODEL_NAME = "return";
        public const string ONSTART_RETURN_NAME = "onstart";
        public const string CALLBACK_EVENT_NAME = "event";
        public const string EVENT_NAME_QUEUE = "emptyqueue";
        public const string EVENT_NAME_SCHEDULE = "schedule";
        public const string EVENT_NAME_START = "start";
        public const string EVENT_NAME_FINISH = "finish";
        #endregion

        private bool _Started = false;

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

        private List<Thread> _SchedulerThreads = new List<Thread>();
        private List<ThreadInfo> _ThreadInfos = new List<ThreadInfo>();
        private int _threadIndexCounter = 0;

        private bool _Continue = true;
        // TODO: With multiple threads we will need synchronization, put the neccessary stuff here
        private object _LockObject = new object();
        private object _WaitingLock = new object();

        private IServiceScopeFactory _ScopeFactory;

        #region Construction
        public IndirectCallService(IServiceScopeFactory scopeFactory, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            _ScopeFactory = scopeFactory;
            int nThreads = ConfiguredTaskThreads;
            for (var i = 0; i < nThreads; i++)
            {
                _ThreadInfos.Add(new ThreadInfo());
                _SchedulerThreads.Add(null);
            }
            // Update from configuration
            _UpdateFromConfiguration(kraftGlobalConfigurationSettings);
        }

        private bool CreateRecreateThread(int index)
        {
            int nThreads = ConfiguredTaskThreads;
            if (index >= 0 && index < nThreads)
            {
                Thread thread = _SchedulerThreads[index];
                if (thread != null)
                {
                    Thread th = new Thread(this.Scheduler);
                    th.IsBackground = true;
                    _ThreadInfos[index] = new ThreadInfo();
                    _SchedulerThreads[index] = th;
                    th.Start();
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Reader for the number of threads with protection threads >= 1 and <= processorcount
        /// </summary>
        private int ConfiguredTaskThreads
        {
            get
            {
                int n = _KraftGlobalConfigurationSettings.GeneralSettings.TaskThreads;
                int maxThreads = _KraftGlobalConfigurationSettings.GeneralSettings.MaxAutoTaskThreads;
                if (maxThreads < 1) { maxThreads = 1; } // Prevent it from being too small
                if (n == 0)
                {
                    // auto choose
                    if (Environment.ProcessorCount > 1)
                    {
                        n = Environment.ProcessorCount - 1;
                        if (maxThreads < n) n = maxThreads;
                    }
                    else
                    {
                        n = 1;
                    }
                    return n;
                }
                if (n < 1) { n = 1; }
                if (n > 1 && n > Environment.ProcessorCount)
                {
                    n = Environment.ProcessorCount;
                }
                return n;
            }
        }
        private void _UpdateFromConfiguration(KraftGlobalConfigurationSettings global)
        {
            if (global != null)
            {
                var scheduler = global.CallScheduler;
                if (scheduler != null)
                {
                    if (scheduler.StartupDelay > 0) STARTUP_DELAY = scheduler.StartupDelay;
                    if (scheduler.RecheckSeconds > 0) TIMEOUT_SECONDS = scheduler.RecheckSeconds;
                    if (scheduler.ResultPreserveSeconds > 0) RESULT_PRESERVE_SECONDS = scheduler.ResultPreserveSeconds;
                    if (scheduler.ScheduleTimeoutSeconds > 0) SCHEDULE_TIMEOUT_SECONDS = scheduler.ScheduleTimeoutSeconds;
                    if (scheduler.WorkerThreads >= 1) WORKER_THREADS = scheduler.WorkerThreads;
                }
            }
        }
        #endregion Construction

        #region Scheduler
        public Task StartAsync(CancellationToken cancellationToken)
        {
            KraftLogger.LogInformation("IndirectCallService: StartAsync executed.");
            for (int i = 0; i < _SchedulerThreads.Count; i++)
            {
                if (!CreateRecreateThread(i))
                {
                    KraftLogger.LogError($"IndirectCallService>StartAsync cannot create thread for index: {i}");
                }
            }
            return Task.FromResult(0);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _Continue = false;
            _ThreadSignal.Set();
            //_SchedulerThreads.ForEach(th =>
            //{
            //    th.Join();
            //    _ThreadSignal.Set();
            //});
            KraftLogger.LogInformation("IndirectCallService:StopAsync executed");
            for (int i = 0; i < _SchedulerThreads.Count; i++)
            {
                Thread th = _SchedulerThreads[i];
                if (th != null)
                {
                    if (th.ThreadState == ThreadState.WaitSleepJoin)
                    {
                        th.Interrupt();
                    }                    
                }
            }

            return Task.FromResult(0);
        }

        private void Scheduler(object index)
        {
            int threadIndex = (int)index;
            ThreadInfo info = new ThreadInfo(); // Empty vessel to avoid checks
            if (threadIndex >= 0 && threadIndex < ConfiguredTaskThreads)
            {
                info = _ThreadInfos[threadIndex]; // Replace it with the right one
            }
            info.ThreadIndex = threadIndex;
            var _timeout_seconds = STARTUP_DELAY; // 
            while (_Continue)
            {
                QueuedTask waiting = null;
                try
                {
                    info.Looping = true;
                    if (_Started)
                    {
                        info.DirectCallAvailable = true;
                        info.TaskPicked = false;
                        lock (_WaitingLock)
                        {
                            if (!_Waiting.TryDequeue(out waiting))
                            {
                                waiting = null;
                            };
                        }
                        if (waiting != null)
                        {
                            waiting.ThreadIndex = threadIndex;
                            info.TaskPicked = true;
                            info.LastTaskPickedAt = DateTime.Now;
                            try
                            {
                                // Discard timeouted tasks
                                if (DateTime.Now - waiting.queued > TimeSpan.FromSeconds(waiting.scheduleTimeout) || waiting.task.status == IndirectCallStatus.Discarded)
                                {
                                    // Move this to finished
                                    waiting.task.status = IndirectCallStatus.Discarded;
                                    lock (_Finished)
                                    {
                                        _Finished.Add(waiting.task.guid, waiting.task);
                                    }
                                    info.LastTaskFinishedAt = DateTime.Now;
                                    continue;
                                }
                                var result = waiting.task;
                                result.started = DateTime.Now;
                                result.status = IndirectCallStatus.Running;
                                lock (_Finished)
                                {
                                    _Finished.Add(result.guid, result);
                                }
                                // Mark call as indirect call
                                if (result.input != null) result.input.CallType = Models.Enumerations.ECallType.ServiceCall;
                                info.Executing = $"{result.input.Module}/{result.input.Nodeset}/{result.input.Nodepath}";
                                info.StartHandler = true;
                                var callbackReturn = CallHandler(HandlerType.started, result.input);
                                info.StartHandler = false;

                                // We depend on the DirectCall to indicate the success in the ReturnModel
                                var returnModel = DirectCallService.Instance.Call(result.input);
                                info.LastTaskFinishedAt = DateTime.Now;
                                result.result = returnModel;
                                result.finished = DateTime.Now;
                                result.status = IndirectCallStatus.Finished;
                                info.FinishHandler = true;
                                CallHandler(HandlerType.finished, result.input, result.result, callbackReturn);
                                info.FinishHandler = false;
                                info.LastTaskCompleted = info.Executing;
                                info.Executing = null;
                                continue; // Check for more tasks before waiting
                            }
                            catch (Exception ex)
                            {
                                KraftLogger.LogError($"Task with id: {waiting.task.guid}", ex, waiting.task.input, waiting.task.result);
                            }
                        }
                        else
                        {
                            // If we are here nothing was in the queue for at least TIMEOUT_SECONDS
                            ScheduleOnEmptyQueue();
                            _ThreadSignal.Reset();
                        }
                    }
                    //_ThreadSignal.WaitOne(TimeSpan.FromSeconds(_timeout_seconds));
                    _ThreadSignal.WaitOne();
                    if (!_Started && DirectCallService.Instance.Call != null)
                    {
                        lock (_LockObject)
                        {
                            _Started = true;
                            _timeout_seconds = TIMEOUT_SECONDS;
                        }
                    }
                }
                catch (ThreadInterruptedException)
                {
                    if (!_Continue)
                    {
                        return; //Service is stopping
                    }
                    if (waiting != null)
                    {
                        waiting.task.status = IndirectCallStatus.Discarded;
                    }
                    if (!CreateRecreateThread(threadIndex))
                    {
                        KraftLogger.LogError($"IndirectCallService>Scheduler(ThreadInterruptedException) cannot re-create thread for index: {threadIndex}");
                    }
                    return;
                }

                // Clean up old results
                CleanUpResults();
            }
            info.LoopingEnded = true;
        }
        private void CleanUpResults()
        {
            lock (_Finished)
            {
                List<Guid> _toclean = new List<Guid>(_Finished.Count / 2);
                foreach (var kv in _Finished)
                {
                    var tsk = kv.Value;
                    if (tsk.finished.HasValue &&
                        tsk.status == IndirectCallStatus.Finished &&
                        (DateTime.Now - tsk.finished) > TimeSpan.FromSeconds(RESULT_PRESERVE_SECONDS))
                    {
                        _toclean.Add(kv.Key);
                    }
                }
                _toclean.ForEach(k => _Finished.Remove(k));
            }
        }


        #endregion Scheduler

        #region Callback helpers
        private void ScheduleOnEmptyQueue()
        {
            var handler_defs = _KraftGlobalConfigurationSettings?.CallScheduler?.OnEmptyQueue;
            if (handler_defs != null && handler_defs.Count > 0)
            {
                foreach (var handler_def in handler_defs)
                {
                    InputModel im = new InputModel();
                    if (!im.ParseAddress(handler_def.Address))
                    {
                        throw new Exception($"Cannot parse address {handler_def.Address}");
                    }
                    im.IsWriteOperation = handler_def.IsWriteOperation;
                    im.RunAs = string.IsNullOrWhiteSpace(handler_def.RunAs) ? null : handler_def.RunAs;
                    im.Data = new Dictionary<string, object>() {
                        { CALLBACK_EVENT_NAME, EVENT_NAME_QUEUE },
                        { "scheduler", this}
                    };
                    this.Call(im, SCHEDULE_TIMEOUT_SECONDS);
                }
            }
        }
        private ReturnModel CallHandler(HandlerType callType, InputModel callModel, ReturnModel retModel = null, ReturnModel callBackReturnModel = null)
        {
            InputModel inputModel = new InputModel();
            CallSchedulerHandler handler = null;
            var data = new Dictionary<string, object>();
            switch (callType)
            {
                case HandlerType.scheduled:
                    handler = callModel?.SchedulerCallHandlers?.OnCallScheduled;
                    if (handler == null) handler = _KraftGlobalConfigurationSettings?.CallScheduler?.CallHandlers?.OnCallScheduled;
                    data.Add(INPUT_MODEL_NAME, callModel.ToDictionary());
                    data.Add(CALLBACK_EVENT_NAME, EVENT_NAME_SCHEDULE);
                    break;
                case HandlerType.started:
                    handler = callModel?.SchedulerCallHandlers?.OnCallStarted;
                    if (handler == null) handler = _KraftGlobalConfigurationSettings?.CallScheduler?.CallHandlers?.OnCallStarted;
                    data.Add(INPUT_MODEL_NAME, callModel.ToDictionary());
                    data.Add(CALLBACK_EVENT_NAME, EVENT_NAME_START);
                    break;
                case HandlerType.finished:
                    handler = callModel?.SchedulerCallHandlers?.OnCallFinished;
                    if (handler == null) handler = _KraftGlobalConfigurationSettings?.CallScheduler?.CallHandlers?.OnCallFinished;
                    data.Add(INPUT_MODEL_NAME, callModel.ToDictionary());
                    data.Add(RETURN_MODEL_NAME, retModel != null ? retModel.ToDictionary() : null);
                    data.Add(ONSTART_RETURN_NAME, callBackReturnModel != null ? callBackReturnModel.ToDictionary() : null);
                    data.Add(CALLBACK_EVENT_NAME, EVENT_NAME_FINISH);
                    break;
                default:
                    return null; // TODO: Ignore missconfigured stuff or may be exception?
            }
            if (handler != null)
            {
                inputModel.Data = data;
                inputModel.ParseAddress(handler.Address);
                inputModel.IsWriteOperation = handler.IsWriteOperation;
                inputModel.RunAs = handler.RunAs;
                inputModel.CallType = Models.Enumerations.ECallType.ServiceCall;
                inputModel.TaskKind = CallTypeConstants.TASK_KIND_CALLBACK;
                var returnModel = DirectCallService.Instance.Call(inputModel);
                // TODO: Devise further usage of the return result.
                // The handling below is probably not the best idea - we have to discuss it.
                if (returnModel != null)
                {
                    if (!returnModel.IsSuccessful)
                    {
                        KraftLogger.LogError($"Error while executing callback: {returnModel.ErrorMessage ?? "unknown"}");
                    }
                    return returnModel;
                }
            }
            return null;

        }
        #endregion


        #region Types

        private enum HandlerType
        {
            scheduled, started, finished
        }
        private record TaskHolder(
            Guid guid,
            InputModel input,
            ReturnModel result,
            IndirectCallStatus status,
            DateTime? started,
            DateTime? finished)
        {

            public IndirectCallStatus status { get; set; } = status;
            public ReturnModel result { get; set; } = result;
            public DateTime? started { get; set; } = started;
            public DateTime? finished { get; set; } = finished;
        }

        private record QueuedTask(TaskHolder task, DateTime queued, int scheduleTimeout)
        {
            public int scheduleTimeout { get; init; } = scheduleTimeout > 0 ? scheduleTimeout : SCHEDULE_TIMEOUT_SECONDS;
            public int ThreadIndex { get; internal set; } = -1;
        }

        #endregion Types

        #region Public access

        public Guid Call(InputModel input, int timeout = 86400)
        {
            //timeout = SCHEDULE_TIMEOUT_SECONDS;
            if (input == null) return Guid.Empty;
            var tsk = new TaskHolder(Guid.NewGuid(), input, null, IndirectCallStatus.Queued, null, null);
            var waiting = new QueuedTask(tsk, DateTime.Now, timeout != 0 ? timeout : SCHEDULE_TIMEOUT_SECONDS);
            lock (_WaitingLock)
            {
                _Waiting.Enqueue(waiting);
            }
            CallHandler(HandlerType.scheduled, input);
            _ThreadSignal.Set();
            return waiting.task.guid;
        }
        public IndirectCallStatus CallStatus(Guid guid)
        {
            TaskHolder task = null;
            lock (_Finished)
            {
                if (_Finished.TryGetValue(guid, out task))
                {
                    return task.status;
                }
                else
                {
                    lock (_WaitingLock)
                    {
                        if (_Waiting.Any(t => t.task.guid == guid))
                        {
                            return IndirectCallStatus.Queued;
                        }
                        else
                        {
                            return IndirectCallStatus.Unavailable;
                        }
                    }
                }
            }
        }
        public ReturnModel GetResult(Guid guid)
        {
            TaskHolder task = null;
            lock (_Finished)
            {
                if (_Finished.TryGetValue(guid, out task))
                {
                    return task.result;
                }
            }
            return null;
        }

        public IIndirectCallInfo GetIndirectCallInfo(Guid guid)
        {
            TaskHolder task = null;
            lock (_Finished)
            {
                if (_Finished.TryGetValue(guid, out task))
                {
                    return new IndirectCallInfo(guid, task.status, task.result, task.started, task.finished);
                }
                else
                {
                    lock (_WaitingLock)
                    {
                        if (_Waiting.Any(t => t.task.guid == guid))
                        {
                            return new IndirectCallInfo(guid, IndirectCallStatus.Queued);
                        }
                        else
                        {
                            return new IndirectCallInfo(guid, IndirectCallStatus.Unavailable);
                        }
                    }
                }
            }
        }
        #endregion

        #region Control interface

        public IIndirectCallerInfo GetIndirectServiceInfo()
        {
            IEnumerable<IIndirectCallInfoEx> waiting = null, finished = null;
            lock (_WaitingLock)
            {
                waiting = _Waiting.Select(qt => new IndirectCallInfoEx(qt.task.guid,
                                                            IndirectCallStatus.Queued,
                                                            qt.task.input,
                                                            qt.task.result,
                                                            null,
                                                            null,
                                                            qt.queued));
            }
            lock (_Finished)
            {
                finished = _Finished.Select(kv => new IndirectCallInfoEx(
                    kv.Value.guid,
                    kv.Value.status,
                    kv.Value.input,
                    kv.Value.result,
                    kv.Value.started,
                    kv.Value.finished
                ));
            }
            return new IndirectCallerInfo(waiting, finished);

        }

        public IIndirectCallerThreads GetIndirectServiceThreadInfo()
        {
            var info = new IndirectCallThreadInfo(_ThreadInfos);
            return info;
        }

        public bool CancelExecution(Guid guid)
        {
            TaskHolder task = null;
            lock (_Finished)
            {
                if (_Finished.TryGetValue(guid, out task))
                {
                    return true;
                }
                else
                {
                    lock (_WaitingLock)
                    {
                        QueuedTask taskHolder = _Waiting.FirstOrDefault(t => t.task.guid == guid);
                        if (taskHolder != null && taskHolder.task != null)
                        {
                            if (taskHolder.task.status == IndirectCallStatus.Queued)
                            {
                                taskHolder.task.status = IndirectCallStatus.Discarded;
                            }
                            else if (taskHolder.task.status == IndirectCallStatus.Running)
                            {
                                if (taskHolder.ThreadIndex > 0)
                                {
                                    Thread th = _SchedulerThreads[taskHolder.ThreadIndex];
                                    if (th != null)
                                    {
                                        th.Interrupt();
                                    }
                                }
                            }

                            return true;
                        }
                    }
                }
            }
            return true;
        }

        #endregion
    }


}
