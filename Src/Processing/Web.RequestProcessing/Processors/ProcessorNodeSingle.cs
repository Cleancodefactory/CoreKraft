using System.Collections.Generic;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Microsoft.AspNetCore.Http;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Utilities.NodeSetService;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Interfaces;
using System.IO;

namespace Ccf.Ck.Processing.Web.Request
{
    internal class ProcessorNodeSingle : ProcessorNodeBase
    {
        public ProcessorNodeSingle(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType, INodeSetService nodeSetService, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, bool preserveBody) : base(httpContext, kraftModuleCollection, requestContentType, nodeSetService, kraftGlobalConfigurationSettings)
        {
        }

        public override IProcessingContextCollection GenerateProcessingContexts(string kraftRequestFlagsKey, ISecurityModel securityModel = null)
        {
            if (securityModel == null)
            {
                if (_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RequireAuthorization)
                {
                    securityModel = new SecurityModel(_HttpContext);
                }
                else
                {
                    securityModel = new SecurityModelMock(_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection);
                }
            }
            InputModelParameters inputModelParameters = CreateBaseInputModelParameters(_KraftGlobalConfigurationSettings, securityModel);
            inputModelParameters = ExtendInputModelParameters(inputModelParameters);
            inputModelParameters.ServerVariables.Add(CallTypeConstants.REQUEST_PROCESSOR, "NodeSingle");
            if (_RequestContentType == ESupportedContentTypes.JSON) {
                inputModelParameters.Data = GetBodyJson<Dictionary<string, object>>(_HttpContext.Request);
            } else if (_RequestContentType == ESupportedContentTypes.FORM_URLENCODED) {
                inputModelParameters.Data = _FormCollection;
            }
            object _GetBodyCallback(string what) {
                if (what == "body") {
                    return RequestBody;
                }
                return null;                
            }
            
            inputModelParameters.LoaderType = GetLoaderType(kraftRequestFlagsKey);
            _ProcessingContextCollection = new ProcessingContextCollection(CreateProcessingContexts(new List<InputModel>() { new InputModel(inputModelParameters, _KraftModuleCollection) { ExtractRequestDataCallback = _GetBodyCallback } })); ;
            return _ProcessingContextCollection;
        }
    }
}
