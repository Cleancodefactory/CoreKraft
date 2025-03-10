﻿using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Processing.Execution;
using Ccf.Ck.Processing.Web.Request.BaseClasses;
using Ccf.Ck.Processing.Web.Request.Primitives;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces.NodeExecution;
using Ccf.Ck.Utilities.NodeSetService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;

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
            processingContext.KraftModule = loadedModule;
            LoadedNodeSet loadedNodeSet = _NodesSetService.LoadNodeSet(
                                                processingContext.InputModel.Module,
                                                processingContext.InputModel.NodeSet,
                                                processingContext.InputModel.Nodepath,
                                                loadedModule);
            CheckValidity(processingContext, loadedModule, loadedNodeSet);
            var security = loadedNodeSet.GetNodeSetSecurity();
            if (!processingContext.CheckSecurity(security))
            {
                throw new UnauthorizedAccessException($"Security requirements not met at NodeSet level: {processingContext.InputModel.Module}/{processingContext.InputModel.NodeSet}/...");
            }
            //PluginAccessorImp<IDataLoaderPlugin> externalService = new PluginAccessorImp<IDataLoaderPlugin>(transactionScopeContext, loadedModule.ModuleSettings);
            //PluginAccessorImp<INodePlugin> customService = new PluginAccessorImp<INodePlugin>(transactionScopeContext, loadedModule.ModuleSettings);
            INodeTaskExecutor taskExecutor = new NodeTaskExecutor(transactionScopeContext, loadedModule.ModuleSettings);
            taskExecutor.ExecuteNodeView(loadedNodeSet, processingContext);
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
            inputModelParameters.ServerVariables.Add(CallTypeConstants.REQUEST_PROCESSOR, "View");
            inputModelParameters.Data = GetBodyJsonAsync<Dictionary<string, object>>(_HttpContext.Request).Result;
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
            processingContext.InputModel = new InputModel(inputModelParameters, _KraftModuleCollection);
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
