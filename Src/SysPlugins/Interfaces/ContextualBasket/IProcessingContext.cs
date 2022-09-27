using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces.Packet;

namespace Ccf.Ck.SysPlugins.Interfaces.ContextualBasket
{
    public interface IProcessingContext
    {
        InputModel InputModel { get; set; }
        IProcessorHandler ProcessorHandler { get; }
        void Execute(ITransactionScopeContext transactionScopeContext);
        IReturnModel ReturnModel { get; set; }

        /// <summary>
        /// // {Security}
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        bool CheckSecurity(Security sec) {
            if (sec == null) return true;
            var secModel = InputModel?.SecurityModel;
            if (secModel == null) return false; // Any non-null security requires a security model against which to be checked
            return secModel.CheckSecurity(sec);
        }
    }
}
