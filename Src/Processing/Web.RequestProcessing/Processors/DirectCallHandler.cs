﻿using System.Collections.Generic;
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
using System.Text;
using Grace.DependencyInjection.Impl.Wrappers;
using Ccf.Ck.Models.Packet;

namespace Ccf.Ck.Processing.Web.Request
{
    public class DirectCallHandler : IProcessorHandler
    {
        Ccf.Ck.Models.DirectCall.InputModel _InputModel;
        KraftModuleCollection _KraftModuleCollection;
        INodeSetService _NodesSetService;

        public DirectCallHandler(Ccf.Ck.Models.DirectCall.InputModel inputModel, KraftModuleCollection kraftModuleCollection, INodeSetService nodeSetService)
        {
            _InputModel = inputModel;
            _KraftModuleCollection = kraftModuleCollection;
            _NodesSetService = nodeSetService;
        }

        public void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext)
        {
            KraftModule loadedModule = _KraftModuleCollection.GetModule(processingContext.InputModel.Module);
            LoadedNodeSet loadedNodeSet = _NodesSetService.LoadNodeSet(
                                                processingContext.InputModel.Module,
                                                processingContext.InputModel.NodeSet,
                                                processingContext.InputModel.Nodepath);
            StringBuilder sb;
            if (!CheckValidity(processingContext, loadedModule, loadedNodeSet, out sb))
            {
                PluginAccessorImp<IDataLoaderPlugin> externalService = new PluginAccessorImp<IDataLoaderPlugin>(transactionScopeContext, loadedModule.ModuleSettings);
                PluginAccessorImp<INodePlugin> customService = new PluginAccessorImp<INodePlugin>(transactionScopeContext, loadedModule.ModuleSettings);
                INodeTaskExecutor taskExecutor = new NodeTaskExecutor(transactionScopeContext, loadedModule.ModuleSettings);
                taskExecutor.Execute(loadedNodeSet, processingContext, externalService, customService);
            }
            else
            {
                processingContext.ReturnModel.Status.IsSuccessful = false;
                processingContext.ReturnModel.Status.StatusResults.Add(new StatusResult(){ Message = sb.ToString(), StatusResultType= SysPlugins.Interfaces.Packet.StatusResultEnum.EStatusResult.StatusResultError});
            }
        }

        public IProcessingContextCollection GenerateProcessingContexts(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, string kraftRequestFlagsKey, ISecurityModel securityModel = null)
        {
            InputModelParameters inputModelParameters = new InputModelParameters();
            inputModelParameters.KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            inputModelParameters.SecurityModel = securityModel;
            inputModelParameters.Module = _InputModel.Module;
            inputModelParameters.Nodeset = _InputModel.Nodeset;
            inputModelParameters.Nodepath = _InputModel.Nodepath;
            inputModelParameters.IsWriteOperation = _InputModel.IsWriteOperation;
            inputModelParameters.QueryCollection = _InputModel.QueryCollection;
            inputModelParameters.Data = _InputModel.Data;
            inputModelParameters.LoaderType = ELoaderType.DataLoader;
            
            IProcessingContext processingContext = new ProcessingContext(this);
            processingContext.InputModel = new InputModel(inputModelParameters);
            List<IProcessingContext> processingContexts = new List<IProcessingContext>(1);
            processingContexts.Add(processingContext);
            return new ProcessingContextCollection(processingContexts);
        }

        public void GenerateResponse()
        {
            throw new System.NotImplementedException();
        }

        private bool CheckValidity(IProcessingContext processingContext, KraftModule module, LoadedNodeSet loadedNodeSet, out StringBuilder sb)
        {
            sb = new StringBuilder();
            bool isError = false;
            if (module == null)
            {
                sb.Append($"Requested module: {processingContext.InputModel.Module} doesn't exist or not loaded.");
                isError = true;
            }

            if (loadedNodeSet == null)
            {
                sb.Append($"Requested nodeset: {processingContext.InputModel.NodeSet} doesn't exist or not loaded.");
                isError = true;
            }
            if (loadedNodeSet.StartNode == null)//Handle errors better and show when a node is addressed but missing.
            {
                sb.Append($"Node: {processingContext.InputModel.Nodepath} from module: {processingContext.InputModel.Module}, nodeset: {processingContext.InputModel.NodeSet} is missing!");
                isError = true;
            }
            return isError;
        }
    }
}
