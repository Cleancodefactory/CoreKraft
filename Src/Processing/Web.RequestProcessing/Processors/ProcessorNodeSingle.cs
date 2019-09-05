using System.Collections.Generic;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Microsoft.AspNetCore.Http;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Utilities.NodeSetService;

namespace Ccf.Ck.Processing.Web.Request
{
    internal class ProcessorNodeSingle : ProcessorNodeBase
    {
        public ProcessorNodeSingle(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType, INodeSetService nodeSetService) : base(httpContext, kraftModuleCollection, requestContentType, nodeSetService)
        {
        }

        public override IProcessingContextCollection GenerateProcessingContexts(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, string kraftRequestFlagsKey, ISecurityModel securityModel = null)
        {
            if (securityModel == null)
            {
                if (kraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RequireAuthorization)
                {
                    securityModel = new SecurityModel(_HttpContext);
                }
                else
                {
                    securityModel = new SecurityModelMock(kraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection);
                }
            }
            InputModelParameters inputModelParameters = CreateBaseInputModelParameters(kraftGlobalConfigurationSettings, securityModel);
            inputModelParameters = ExtendInputModelParameters(inputModelParameters);
            inputModelParameters.Data = GetBodyJson<Dictionary<string, object>>(_HttpContext.Request);
            inputModelParameters.FormCollection = _FormCollection;
            inputModelParameters.LoaderType = GetLoaderType(kraftRequestFlagsKey);
            _ProcessingContextCollection = new ProcessingContextCollection(CreateProcessingContexts(new List<InputModel> { new InputModel(inputModelParameters) }));
            return _ProcessingContextCollection;
        }
    }
}
