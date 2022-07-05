using System.Collections.Generic;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Microsoft.AspNetCore.Http;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Processing.Web.Request.Primitives;
using Microsoft.AspNetCore.Routing;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.Utilities.NodeSetService;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.NodeExecution;
using Ccf.Ck.Processing.Execution;
using Ccf.Ck.Models.NodeSet;

namespace Ccf.Ck.Processing.Web.Request
{
    internal class ProcessorView : ProcessorNodeBase
    {
        public ProcessorView(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType, INodeSetService nodeSetService, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) : base(httpContext, kraftModuleCollection, requestContentType, nodeSetService, kraftGlobalConfigurationSettings)
        {
        }

        public override void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext)
        {
            KraftModule loadedModule = _KraftModuleCollection.GetModule(processingContext.InputModel.Module);
            LoadedNodeSet loadedNodeSet = _NodesSetService.LoadNodeSet(
                                                processingContext.InputModel.Module,
                                                processingContext.InputModel.NodeSet,
                                                processingContext.InputModel.Nodepath,
                                                loadedModule);
            if (CheckValidity(processingContext, loadedModule, loadedNodeSet))
            {
                PluginAccessorImp<IDataLoaderPlugin> externalService = new PluginAccessorImp<IDataLoaderPlugin>(transactionScopeContext, loadedModule.ModuleSettings);
                PluginAccessorImp<INodePlugin> customService = new PluginAccessorImp<INodePlugin>(transactionScopeContext, loadedModule.ModuleSettings);
                INodeTaskExecutor taskExecutor = new NodeTaskExecutor(transactionScopeContext, loadedModule.ModuleSettings);
                taskExecutor.ExecuteNodeView(loadedNodeSet, processingContext);
            }
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
            inputModelParameters.Data = GetBodyJson<Dictionary<string, object>>(_HttpContext.Request);
            inputModelParameters.FormCollection = _FormCollection;
            inputModelParameters.LoaderType = GetLoaderType(kraftRequestFlagsKey);
            if (inputModelParameters.LoaderType == ELoaderType.None)
            {
                inputModelParameters.LoaderType = ELoaderType.ViewLoader;
            }
            RouteData routeData = _HttpContext.GetRouteData();
            if (routeData != null)
            {
                inputModelParameters.BindingKey = routeData.Values[Constants.RouteSegmentConstants.RouteBindingkey] as string;
            }
            IProcessingContext processingContext = new ProcessingContext(this);
            processingContext.InputModel = new InputModel(inputModelParameters);
            List<IProcessingContext> processingContexts = new List<IProcessingContext>(1);
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
