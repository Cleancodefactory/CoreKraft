using System.Collections.Generic;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.Settings.Modules;
using System;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Processing.Web.Request.Primitives;
using Microsoft.AspNetCore.Routing;
using Ccf.Ck.Utilities.Generic;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Utilities.NodeSetService;
using Ccf.Ck.Models.Interfaces;

namespace Ccf.Ck.Processing.Web.Request
{
    public class ProcessorSignal : ProcessorNodeBase
    {
        public ProcessorSignal(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType, INodeSetService nodeSetService, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) : base(httpContext, kraftModuleCollection, requestContentType, nodeSetService, kraftGlobalConfigurationSettings)
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
            inputModelParameters.ServerVariables.Add(CallTypeConstants.REQUEST_PROCESSOR, "Signal");
            inputModelParameters.ServerVariables[CallTypeConstants.REQUEST_CALL_TYPE] = (int)ECallType.Signal;
            if (_RequestContentType == ESupportedContentTypes.JSON)
            {
                inputModelParameters.Data = GetBodyJson<Dictionary<string, object>>(_HttpContext.Request);
            }
            else
            {
                inputModelParameters.Data = _FormCollection;
            }
            inputModelParameters.FormCollection = _FormCollection;
            inputModelParameters.LoaderType = GetLoaderType(kraftRequestFlagsKey);

            RouteData routeData = _HttpContext.GetRouteData();
            if (routeData != null)
            {
                string routeDataKey = routeData.DataTokens["key"]?.ToString()?.ToLower();
                if (!string.IsNullOrEmpty(routeDataKey))
                {
                    string signal = routeData.Values[Constants.RouteSegmentConstants.RouteModuleSignalParameter]?.ToString();
                    bool isWriteOperation = routeDataKey.Equals(Constants.RouteSegmentConstants.RouteDataTokenSignalWrite);
                    return PrepareSignals(inputModelParameters.Module, signal, isWriteOperation, inputModelParameters);
                }
            }
            //Return only empty collection
            return new ProcessingContextCollection(new List<IProcessingContext>());
        }

        public override void GenerateResponse()
        {
            HttpResponseBuilder responseBuilder = new XmlPacketResponseBuilder(_ProcessingContextCollection);
            responseBuilder.GenerateResponse(_HttpContext);
        }

        private IProcessingContextCollection PrepareSignals(string moduleName, string signalKey, bool isWriteOperation, InputModelParameters inputModelParametersTemplate)
        {
            List<IProcessingContext> resultContexts = new List<IProcessingContext>();
            bool isMaintenance = false;
            InputModelParameters inputModelParameters = inputModelParametersTemplate.DeepClone();

            if (string.IsNullOrEmpty(moduleName) || moduleName.Equals("null", StringComparison.OrdinalIgnoreCase))//Apply for all modules
            {
                foreach (KraftModule kraftModule in _KraftModuleCollection.GetSortedModules())
                {
                    KraftModuleSignal signal = FindSignal(kraftModule, signalKey);
                    if (signal != null)
                    {
                        isMaintenance = signal.Maintenance;
                        if (kraftModule.KraftModuleRootConf.Signals.Count > 0)
                        {
                            resultContexts.Add(Signal2ProcessingContext(inputModelParameters, kraftModule, signal, isWriteOperation));
                        }
                    }
                }
            }
            else
            {
                KraftModule kraftModule = _KraftModuleCollection.GetSortedModules().Find(m => m.Key.Equals(moduleName, StringComparison.OrdinalIgnoreCase));
                KraftModuleSignal signal = FindSignal(kraftModule, signalKey);
                if (signal != null)
                {
                    isMaintenance = signal.Maintenance;
                    resultContexts.Add(Signal2ProcessingContext(inputModelParameters, kraftModule, signal, isWriteOperation));
                }
            }
            _ProcessingContextCollection = new ProcessingContextCollection(resultContexts, isMaintenance);
            return _ProcessingContextCollection;
        }

        private IProcessingContext Signal2ProcessingContext(InputModelParameters inputModelParameters, KraftModule kraftModule, KraftModuleSignal signal, bool isWriteOperation)
        {
            inputModelParameters.Module = kraftModule.Key;
            inputModelParameters.Nodeset = signal?.NodeSet;
            inputModelParameters.Nodepath = signal?.NodePath;
            inputModelParameters.IsWriteOperation = isWriteOperation;
            if (inputModelParameters.LoaderType == ELoaderType.None)
            {
                inputModelParameters.LoaderType = ELoaderType.DataLoader;
            }
            IProcessingContext processingContext = new ProcessingContext(this)
            {
                InputModel = new InputModel(inputModelParameters, _KraftModuleCollection)
            };
            return processingContext;
        }

        private KraftModuleSignal FindSignal(KraftModule kraftModule, string signalKey)
        {
            return kraftModule.KraftModuleRootConf?.Signals?.FirstOrDefault(k => k.Key.Equals(signalKey, StringComparison.OrdinalIgnoreCase));
        }
    }
}
