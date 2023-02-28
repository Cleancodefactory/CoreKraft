using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.KraftModule;
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

        KraftModule KraftModule { get; set; }

        /// <summary>
        /// // {Security}
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        bool CheckSecurity(Security sec) {
            if (sec == null) return true;
            ISecurityModel secModel = new SecurityModelCopy(InputModel?.SecurityModel);
            return secModel.CheckSecurity(sec);
        }
        bool NeedsAuthentication(Security sec) {
            if (sec == null) return false;
            ISecurityModel secModel = new SecurityModelCopy(InputModel?.SecurityModel);
            return secModel.NeedsAuthentication(sec);
        }
    }
}
