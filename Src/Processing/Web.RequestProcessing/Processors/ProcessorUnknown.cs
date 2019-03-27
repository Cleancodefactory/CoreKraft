using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Packet;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;

namespace Ccf.Ck.Processing.Web.Request
{
    internal class ProcessorUnknown : ProcessorBase
    {
        public ProcessorUnknown(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType) : base(httpContext, kraftModuleCollection, requestContentType)
        {
        }

        public override void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext)
        {
            processingContext.ReturnModel = new ReturnModel();
            processingContext.ReturnModel.Status.IsSuccessful = false;
            processingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { Message = "Unknown request type!", StatusResultType = EStatusResult.StatusResultError });
        }

        public override IProcessingContextCollection GenerateProcessingContexts(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, string kraftRequestFlagsKey, ISecurityModel securityModel = null)
        {
            IProcessingContext processingContext = new ProcessingContext(this);
            List<IProcessingContext> processingContexts = new List<IProcessingContext>();
            processingContexts.Add(processingContext);
            _ProcessingContextCollection = new ProcessingContextCollection(processingContexts);
            return _ProcessingContextCollection;
        }

        public override void GenerateResponse()
        {
            HttpResponseBuilder responseBuilder = new XmlPacketResponseBuilder(_ProcessingContextCollection);
            responseBuilder.GenerateResponse(_HttpContext);
        }
    }
}
