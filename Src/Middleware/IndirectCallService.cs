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
    internal class IndirectCallService : IHostedService
    {

        private IServiceScopeFactory _ScopeFactory;
        public IndirectCallService(IServiceScopeFactory scopeFactory)
        {
            _ScopeFactory = scopeFactory;
            //_Timers = new List<Timer>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }


}
