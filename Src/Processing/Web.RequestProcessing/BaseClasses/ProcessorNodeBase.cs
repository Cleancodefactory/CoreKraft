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
using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Interfaces;

namespace Ccf.Ck.Processing.Web.Request.BaseClasses
{
    /// <summary>
    /// This class is common for all the requests comming normaly from the WEB
    /// </summary>
    public abstract class ProcessorNodeBase : ProcessorBase
    {
        protected Dictionary<string, object> _QueryCollection;
        protected Dictionary<string, object> _HeaderCollection;
        protected Dictionary<string, object> _FormCollection;
        protected Dictionary<string, object> _ServerCollection;
        protected INodeSetService _NodesSetService;

        public ProcessorNodeBase(HttpContext httpContext, KraftModuleCollection kraftModuleCollection, ESupportedContentTypes requestContentType, INodeSetService nodeSetService, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings) : base(httpContext, kraftModuleCollection, requestContentType, kraftGlobalConfigurationSettings)
        {
            _QueryCollection = httpContext.Request.Query.Convert2Dictionary();
            _HeaderCollection = httpContext.Request.Headers.Convert2Dictionary();
            _FormCollection = (httpContext.Request.HasFormContentType) ? httpContext.Request?.Form?.Convert2Dictionary() : new Dictionary<string, object>();
            _ServerCollection = new Dictionary<string, object>();
            _ServerCollection.Add("REMOTE_ADDR", httpContext.Connection.RemoteIpAddress);
            _ServerCollection.Add("SERVER_HOST_KEY", kraftGlobalConfigurationSettings.GeneralSettings.ServerHostKey);
            _NodesSetService = nodeSetService;
        }

        public override void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext)
        {
            KraftModule loadedModule = _KraftModuleCollection.GetModule(processingContext.InputModel.Module);
            LoadedNodeSet loadedNodeSet = _NodesSetService.LoadNodeSet(
                                                processingContext.InputModel.Module,
                                                processingContext.InputModel.NodeSet,
                                                processingContext.InputModel.Nodepath,
                                                loadedModule);
            if (CheckValidity(processingContext, loadedModule, loadedNodeSet)) //also redirects if require authorization is true
            {
                // {Security} Check security on nodeset level
                var security = loadedNodeSet.GetNodeSetSecurity();
                if (!processingContext.CheckSecurity(security))
                {
                    throw new UnauthorizedAccessException($"Security requirements not met at NodeSet level: {processingContext.InputModel.Module}/{processingContext.InputModel.NodeSet}/...");
                }
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
            inputModelParameters.ServerVariables = _ServerCollection;
            inputModelParameters.KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            inputModelParameters.SecurityModel = securityModel;
            if (inputModelParameters.ServerVariables == null) inputModelParameters.ServerVariables = new Dictionary<string, object>();
            inputModelParameters.ServerVariables.Add(CallTypeConstants.REQUEST_CALL_TYPE, (int)ECallType.WebRequest);
            inputModelParameters.ServerVariables.Add(CallTypeConstants.TASK_KIND, CallTypeConstants.TASK_KIND_WEBREQUEST);
            return inputModelParameters;
        }

        protected InputModelParameters ExtendInputModelParameters(InputModelParameters inputModelParameters)
        {
            RouteDataPrimitives routeDataPrimitives = GetRouteData();
            inputModelParameters.Module = routeDataPrimitives.Module;
            inputModelParameters.Nodeset = routeDataPrimitives.Nodeset;
            inputModelParameters.Nodepath = routeDataPrimitives.Nodepath;
            inputModelParameters.IsWriteOperation = routeDataPrimitives.IsWriteOperation;
            inputModelParameters.ReadAction = routeDataPrimitives.ReadAction;
            return inputModelParameters;
        }

        protected ELoaderType GetLoaderType(string loaderTypeKey)
        {
            if (_QueryCollection.ContainsKey(loaderTypeKey))
            {
                string sysrequestcontent = _QueryCollection[loaderTypeKey]?.ToString();
                return GetLoaderContent(sysrequestcontent);
            }
            return ELoaderType.DataLoader;
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
            ELoaderType loaderType = ELoaderType.None; //We are here because something with the name sysrequestcontent has been found
            if (!string.IsNullOrEmpty(sysrequestcontent))
            {
                if (!int.TryParse(sysrequestcontent, out int enumValueInt))
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
                Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.NotFound, _KraftGlobalConfigurationSettings, $"You have to specify a loader type.");
                return false;
            }
            if (module == null)
            {
                Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.NotFound, _KraftGlobalConfigurationSettings, $"Requested module: {processingContext.InputModel.Module} doesn't exist or not loaded.");
                return false;
            }

            if (loadedNodeSet == null)
            {
                Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.NotFound, _KraftGlobalConfigurationSettings, $"Requested nodeset: {processingContext.InputModel.NodeSet} doesn't exist or not loaded.");
                return false;
            }
            if (loadedNodeSet.StartNode == null)//Handle errors better and show when a node is addressed but missing.
            {
                string error = $"Node: {processingContext.InputModel.Nodepath} from module: {processingContext.InputModel.Module}, nodeset: {processingContext.InputModel.NodeSet} is missing!";
                KraftLogger.LogError(error);
                Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.InternalServerError, _KraftGlobalConfigurationSettings, error);
                return false;
            }
            var startSec = loadedNodeSet.GetStartSecurity();
            if (processingContext.NeedsAuthentication(startSec))
            {
                Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.Unauthorized, null);
                return false;
            }
            //If authentication is required but the user is not logged in redirect to authentication
            //or if RequireAuthorizationAnyEndpoint is enabled
            if (_KraftGlobalConfigurationSettings.GeneralSettings.AuthorizationSection.RequireAuthorizationAnyEndpoint || 
                loadedNodeSet.StartNode.RequireAuthentication)
            {
                if (!processingContext.InputModel.SecurityModel.IsAuthenticated)
                {
                    Utilities.ExtensionMethods.KraftResult(_HttpContext, HttpStatusCode.Unauthorized, null);
                    return false;
                }
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
                if (!isWriteOperation)
                {
                    if (routeData.DataTokens["key"] != null && routeData.DataTokens["key"].Equals(Constants.RouteSegmentConstants.RouteDataTokenNew))
                    {
                        routeDataPrimitives.ReadAction = EReadAction.New;
                    }                    
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
            internal EReadAction ReadAction { get; set; } = EReadAction.Default;
            internal bool IsWriteOperation { get; set; }
            internal string Module { get; set; }
            internal string Nodeset { get; set; }
            internal string Nodepath { get; set; }
        }
    }
}