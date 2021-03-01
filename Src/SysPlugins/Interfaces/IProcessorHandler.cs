using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IProcessorHandler
    {
        void GenerateResponse();
        void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext);
        IProcessingContextCollection GenerateProcessingContexts(string kraftRequestFlagsKey, ISecurityModel securityModel = null);
    }
}
