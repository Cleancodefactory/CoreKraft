using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.SysPlugins.Interfaces;
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
    public class IndirectCallService : IIndirectCallService, IHostedService {
        public const int TIMEOUT_SECONDS = 120;
        public const int RESULT_PRESERVE_SECONDS = 3600;
        public const int SCHEDULE_TIMEOUT_SECONDS = 84000;
        public const int WORKER_THREADS = 2;


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
        private IServiceScopeFactory _ScopeFactory;

        #region Construction
        public IndirectCallService(IServiceScopeFactory scopeFactory) {
            _ScopeFactory = scopeFactory;
            _SchedulerThread = new Thread(new ThreadStart(this.Scheduler));
            
            
        }

        #endregion Construction

        #region Scheduler
        public Task StartAsync(CancellationToken cancellationToken) {
            _SchedulerThread.Start();
            return Task.FromResult(0);
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            _Continue = false;
            _ThreadSignal.Set();
            _SchedulerThread.Join();
            return Task.FromResult(0);
        }
        private void Scheduler() {
            while (_Continue) {
                QueuedTask waiting = null;
                lock (_Waiting) {
                    if (!_Waiting.TryDequeue(out waiting))
                    {
                        waiting = null;
                    };
                }
                if (waiting != null) {
                    // Discard timeouted tasks
                    if (DateTime.Now - waiting.queued > TimeSpan.FromSeconds(waiting.scheduleTimeout)) {
                        // Move this to finished
                        waiting.task.status = IndirectCallStatus.Discarded;
                        lock(_Finished) {
                            _Finished.Add(waiting.task.guid, waiting.task);
                        }
                        continue;
                    }
                    var result = waiting.task;
                    result.started = DateTime.Now;
                    result.status = IndirectCallStatus.Running;
                    lock(_Finished)
                    {
                        _Finished.Add(result.guid, result);
                    }
                    // We depend on the DirectCall to indicate the success in the ReturnModel
                    var returnModel = DirectCallService.Instance.Call(result.input);
                    result.result = returnModel;
                    result.finished = DateTime.Now;
                    result.status = IndirectCallStatus.Finished;
                    continue; // Check for more tasks before waiting
                }
                _ThreadSignal.WaitOne(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
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

        private record QueuedTask(TaskHolder task, DateTime queued, int scheduleTimeout = SCHEDULE_TIMEOUT_SECONDS);

        

        #endregion Types

        #region Public access

        public Guid Call(InputModel input, int timeout = SCHEDULE_TIMEOUT_SECONDS)
        {
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

    }


}
