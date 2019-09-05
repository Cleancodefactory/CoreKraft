using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.SysPlugins.Interfaces.Packet;

namespace Ccf.Ck.SysPlugins.Interfaces.ContextualBasket
{
    public interface IProcessingContext
    {
        InputModel InputModel { get; set; }
        IProcessorHandler ProcessorHandler { get; }
        void Execute(ITransactionScopeContext transactionScopeContext);
        IReturnModel ReturnModel { get; set; }
    }
}
