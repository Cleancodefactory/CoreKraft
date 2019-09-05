using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Processing.Web.Request.Utilities;
using Microsoft.AspNetCore.Routing;
using Ccf.Ck.Processing.Web.Request.Primitives;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.NodeExecution;
using Ccf.Ck.Processing.Execution;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Utilities.NodeSetService;
using System.Net;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using System.Linq;

namespace Ccf.Ck.Processing.Web.Request.BaseClasses
{
    public abstract class ProcessorNodeBase : ProcessorBase
    {
        protected Dictionary<string, object> _QueryCollection;
        protected Dictionary<string, object> _HeaderCollection;
        protected Dictionary<string, object> _FormCollection;
        protected INodeSetService _NodesSetService;

        public ProcessorNodeBase(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType, INodeSetService nodeSetService) : base(httpContext, kraftModuleCollection, requestContentType)
        {
            _QueryCollection = httpContext.Request.Query.Convert2Dictionary();
            _HeaderCollection = httpContext.Request.Headers.Convert2Dictionary();
            _FormCollection = (httpContext.Request.HasFormContentType) ? httpContext.Request?.Form?.Convert2Dictionary() : new Dictionary<string, object>();
            _NodesSetService = nodeSetService;
        }

        public override void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext)
        {
            KraftModule loadedModule = _KraftModuleCollection.GetModule(processingContext.InputModel.Module);
            LoadedNodeSet loadedNodeSet = _NodesSetService.LoadNodeSet(
                                                processingContext.InputModel.Module,
                                                processingContext.InputModel.NodeSet,
                                                processingContext.InputModel.Nodepath);
            if (CheckValidity(processingContext, loadedModule, loadedNodeSet))
            {
                PluginAccessorImp<IDataLoaderPlugin> externalService = new PluginAccessorImp<IDataLoaderPlugin>(transactionScopeContext, loadedModule.ModuleSettings);
                PluginAccessorImp<INodePlugin> customService = new PluginAccessorImp<INodePlugin>(transactionScopeContext, loadedModule.ModuleSettings);
                INodeTaskExecutor taskExecutor = new NodeTaskExecutor(transactionScopeContext, loadedModule.ModuleSettings);
                taskExecutor.Execute(loadedNodeSet, processingContext, externalService, customService);
            }
        }

        public override void GenerateResponse()
        {
            IProcessingContext processingContext = _ProcessingContextCollection.ProcessingContexts.FirstOrDefault();
            HttpResponseBuilder responseBuilder = new XmlPacketResponseBuilder(_ProcessingContextCollection);
            if (processingContext != null)
            {
                if (processingContext.ReturnModel.ResponseBuilder is HttpResponseBuilder responseBuilderInternal)
                {
                    responseBuilder = responseBuilderInternal;
                }
            }
            responseBuilder.GenerateResponse(_HttpContext);
        }

        protected InputModelParameters CreateBaseInputModelParameters(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, ISecurityModel securityModel)
        {
            InputModelParameters inputModelParameters = new InputModelParameters();
            inputModelParameters.QueryCollection = _QueryCollection;
            inputModelParameters.HeaderCollection = _HeaderCollection;
            inputModelParameters.FormCollection = _FormCollection;
            inputModelParameters.KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            inputModelParameters.SecurityModel = securityModel;
            return inputModelParameters;
        }

        protected InputModelParameters ExtendInputModelParameters(InputModelParameters inputModelParameters)
        {
            RouteDataPrimitives routeDataPrimitives = GetRouteData();
            inputModelParameters.Module = routeDataPrimitives.Module;
            inputModelParameters.Nodeset = routeDataPrimitives.Nodeset;
            inputModelParameters.Nodepath = routeDataPrimitives.Nodepath;
            inputModelParameters.IsWriteOperation = routeDataPrimitives.IsWriteOperation;
            return inputModelParameters;
        }

        protected ELoaderType GetLoaderType(string loaderTypeKey)
        {
            if (_QueryCollection.ContainsKey(loaderTypeKey))
            {
                string sysrequestcontent = _QueryCollection[loaderTypeKey]?.ToString();
                return GetLoaderContent(sysrequestcontent);
            }
            return ELoaderType.None;
        }

        protected List<IProcessingContext> CreateProcessingContexts(List<InputModel> inputModels)
        {
            List<IProcessingContext> processingContexts = new List<IProcessingContext>(inputModels.Count);
            foreach (InputModel inputModel in inputModels)
            {
                IProcessingContext processingContext = new ProcessingContext(this);
                processingContext.InputModel = inputModel;
                processingContexts.Add(processingContext);
            }
            return processingContexts;
        }

        protected ELoaderType GetLoaderContent(string sysrequestcontent)
        {
            ELoaderType loaderType = ELoaderType.None;
            if (!string.IsNullOrEmpty(sysrequestcontent))
            {
                int enumValueInt;
                if (!int.TryParse(sysrequestcontent, out enumValueInt))
                {
                    enumValueInt = int.Parse(sysrequestcontent, System.Globalization.NumberStyles.HexNumber);
                }
                else
                {
                    enumValueInt = int.Parse(enumValueInt.ToString(), System.Globalization.NumberStyles.HexNumber);
                }
                Enum.TryParse<ELoaderType>(enumValueInt.ToString(), out loaderType);
            }
            return loaderType;
        }

        protected bool CheckValidity(IProcessingContext processingContext, KraftModule module, LoadedNodeSet loadedNodeSet)
        {
            if (processingContext.InputModel.LoaderType == ELoaderType.None)
            {
                Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.NotFound, $"You have to specify a loader type.");
                return false;
            }
            if (module == null)
            {
                Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.NotFound, $"Requested module: {processingContext.InputModel.Module} doesn't exist or not loaded.");
                return false;
            }

            if (loadedNodeSet == null)
            {
                Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.NotFound, $"Requested nodeset: {processingContext.InputModel.NodeSet} doesn't exist or not loaded.");
                return false;
            }
            //If authentication is required but the user is not logged in redirect to authentication
            if (loadedNodeSet.StartNode.RequireAuthentication && !processingContext.InputModel.SecurityModel.IsAuthenticated)
            {
                Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.Unauthorized, null);
                return false;
            }
            return true;
        }
        protected RouteDataPrimitives GetRouteData()
        {
            RouteData routeData = _HttpContext.GetRouteData();
            RouteDataPrimitives routeDataPrimitives = new RouteDataPrimitives();
            if (routeData.Values != null)
            {
                bool isWriteOperation = false;
                if (routeData.DataTokens["key"] != null && routeData.DataTokens["key"].Equals(Constants.RouteSegmentConstants.RouteDataTokenWrite))
                {
                    isWriteOperation = true;
                }
                routeDataPrimitives.IsWriteOperation = isWriteOperation;
                routeDataPrimitives.Module = routeData.Values[Constants.RouteSegmentConstants.RouteModule] as string;
                routeDataPrimitives.Nodeset = routeData.Values[Constants.RouteSegmentConstants.RouteNodeset] as string;
                routeDataPrimitives.Nodepath = routeData.Values[Constants.RouteSegmentConstants.RouteNodepath] as string;
            }
            return routeDataPrimitives;
        }

        public class RouteDataPrimitives
        {
            internal bool IsWriteOperation { get; set; }
            internal string Module { get; set; }
            internal string Nodeset { get; set; }
            internal string Nodepath { get; set; }
        }
    }
}