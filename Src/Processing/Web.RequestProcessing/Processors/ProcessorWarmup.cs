using System.Collections.Generic;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Microsoft.AspNetCore.Http;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Interfaces;

namespace Ccf.Ck.Processing.Web.Request
{
    internal class ProcessorWarmup : ProcessorBase
    {
        public ProcessorWarmup(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) : base(httpContext, kraftModuleCollection, requestContentType, kraftGlobalConfigurationSettings)
        {            
        }

        public override void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext)
        {
            foreach (KraftModule kraftModule in _KraftModuleCollection.GetSortedModules())
            {
                BundleCollection.Instance.Profile(kraftModule.Key).KraftScripts().Render();
                BundleCollection.Instance.Profile(kraftModule.Key).KraftStyles().Render();
            }
            foreach (Profile profile in BundleCollection.Instance.Profiles.Values)
            {
                profile.Styles?.Render();
                profile.Scripts?.Render();
            }
        }

        public override IProcessingContextCollection GenerateProcessingContexts(string kraftRequestFlagsKey, ISecurityModel securityModel = null)
        {
            IProcessingContext processingContext = new ProcessingContext(this);
            processingContext.InputModel = new InputModel(new InputModelParameters());
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
