using Ccf.Ck.Models.DirectCall;
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
    public class IndirectCallService : IHostedService
    {
        public const int TIMEOUT_SECONDS = 120;
        public const int RESULT_PRESERVE_SECONDS = 3600;


        private Queue<InputModel> _Tasks = new Queue<InputModel>();
        private long _id = 0;
        private Dictionary<long, TaskHolder> _Results = new Dictionary<long, TaskHolder>();
        private AutoResetEvent _ThreadSignal = new AutoResetEvent(true);
        private Thread _WorkerThread;
        private bool _Continue = true;
        private IServiceScopeFactory _ScopeFactory;
        public IndirectCallService(IServiceScopeFactory scopeFactory)
        {
            _ScopeFactory = scopeFactory;
            _WorkerThread = new Thread(new ThreadStart(this.Worker));
            //_Timers = new List<Timer>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _WorkerThread.Start();
            return Task.FromResult(0);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _Continue = false;
            _ThreadSignal.Set();
            _WorkerThread.Join();
            return Task.FromResult(0);
        }
        private void Worker()
        {
            while (_Continue)
            {
                InputModel input = null;
                lock (_Tasks)
                {
                    if (!_Tasks.TryDequeue(out input))
                    {
                        input = null;
                    };
                }
                if (input != null)
                {
                    long id = Interlocked.Increment(ref _id);
                    var result = new TaskHolder(null, id, DateTime.Now, null);
                    lock(_Results)
                    {
                        _Results.Add(id, result);
                    }
                    var returnModel = DirectCallService.Instance.Call(input);
                    result.result = returnModel;
                    result.finished = DateTime.Now;
                    continue; // Check for more tasks before waiting
                }
                _ThreadSignal.WaitOne(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
                // Clean up old results
                lock(_Results) {
                    List<long> _toclean = new List<long>(_Results.Count / 2);
                    foreach (var kv in _Results)
                    {
                        if (kv.Value.finished.HasValue && (DateTime.Now - kv.Value.finished) > TimeSpan.FromSeconds(RESULT_PRESERVE_SECONDS))
                        {
                            _toclean.Add(kv.Key);
                        }
                    }
                    _toclean.ForEach(k => _Results.Remove(k));
                }
            }
        }

        private record TaskHolder(ReturnModel result,long id,DateTime started,DateTime? finished)
        {
            public ReturnModel result { get; set; } = result;
            public DateTime? finished { get; set; } = finished;

        };
        
    }


}
