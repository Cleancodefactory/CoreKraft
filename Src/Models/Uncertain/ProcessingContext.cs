using Ccf.Ck.Models.Packet;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using System.Threading;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.Packet;

namespace Ccf.Ck.Models.ContextBasket
{
    public class ProcessingContext : IProcessingContext
    {
        private static long _TraceId = 0;

        public ProcessingContext(IProcessorHandler processorHandler)
        {
            ProcessorHandler = processorHandler;
            ReturnModel = new ReturnModel();
#if NO_TRACE_ID
                // Do not generate unique id on each processing context
#else
            _TraceId = Interlocked.Increment(ref _TraceId);
#endif
        }

        public InputModel InputModel { get; set; }
        public IProcessorHandler ProcessorHandler { get; set; }
        public IReturnModel ReturnModel { get; set; }
        public long TraceId
        {
            get
            {
                return _TraceId;
            }
        }

        public void Execute(ITransactionScopeContext transactionScopeContext)
        {
            ProcessorHandler.Execute(this, transactionScopeContext);
        }
    }
}
