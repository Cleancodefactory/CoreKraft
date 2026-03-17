using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Packet;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Ccf.Ck.Processing.Web.Request.Primitives;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;

namespace Ccf.Ck.Processing.Web.Request
{
    internal class ProcessorUnknown : ProcessorBase
    {
        public ProcessorUnknown(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) : base(httpContext, kraftModuleCollection, requestContentType, kraftGlobalConfigurationSettings)
        {
        }

        public override void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext)
        {
            processingContext.ReturnModel = new ReturnModel();
            processingContext.ReturnModel.Status.IsSuccessful = false;
            processingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { Message = "Unknown request type!", StatusResultType = EStatusResult.StatusResultError });
        }

        public override IProcessingContextCollection GenerateProcessingContexts(string kraftRequestFlagsKey, ISecurityModel securityModel = null)
        {
            RouteData routeData = _HttpContext.GetRouteData();
            InputModelParameters inputModelParameters = new InputModelParameters
            {
                Module = routeData?.Values?[Constants.RouteSegmentConstants.RouteModule]?.ToString(),
                Nodeset = routeData?.Values?[Constants.RouteSegmentConstants.RouteNodeset]?.ToString(),
                Nodepath = routeData?.Values?[Constants.RouteSegmentConstants.RouteNodepath]?.ToString(),
                BindingKey = routeData?.Values?[Constants.RouteSegmentConstants.RouteBindingkey]?.ToString(),
                IsWriteOperation = false,
                LoaderType = ELoaderType.None,
                KraftGlobalConfigurationSettings = _KraftGlobalConfigurationSettings,
                QueryCollection = new Dictionary<string, object>(),
                HeaderCollection = new Dictionary<string, object>(),
                FormCollection = new Dictionary<string, object>(),
                ServerVariables = new Dictionary<string, object> { { CallTypeConstants.REQUEST_PROCESSOR, "Unknown" } },
                SecurityModel = securityModel,
                Data = new Dictionary<string, object>()
            };

            IProcessingContext processingContext = new ProcessingContext(this)
            {
                InputModel = new InputModel(inputModelParameters, _KraftModuleCollection)
            };
            List<IProcessingContext> processingContexts = new List<IProcessingContext> { processingContext };
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
