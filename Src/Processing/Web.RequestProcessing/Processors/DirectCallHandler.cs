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
using System.Text;
using Grace.DependencyInjection.Impl.Wrappers;
using Ccf.Ck.Models.Packet;
using Ccf.Ck.Libs.Logging;

namespace Ccf.Ck.Processing.Web.Request
{
    public class DirectCallHandler : IProcessorHandler
    {
        Ccf.Ck.Models.DirectCall.InputModel _InputModel;
        KraftModuleCollection _KraftModuleCollection;
        INodeSetService _NodesSetService;
        KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;

        public DirectCallHandler(Ccf.Ck.Models.DirectCall.InputModel inputModel, KraftModuleCollection kraftModuleCollection, INodeSetService nodeSetService, KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _InputModel = inputModel;
            _KraftModuleCollection = kraftModuleCollection;
            _NodesSetService = nodeSetService;
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
        }

        public void Execute(IProcessingContext processingContext, ITransactionScopeContext transactionScopeContext)
        {
            KraftModule loadedModule = _KraftModuleCollection.GetModule(processingContext.InputModel.Module);
            LoadedNodeSet loadedNodeSet = default(LoadedNodeSet);
            if (loadedModule != null) {
                loadedNodeSet = _NodesSetService.LoadNodeSet(
                                                    processingContext.InputModel.Module,
                                                    processingContext.InputModel.NodeSet,
                                                    processingContext.InputModel.Nodepath,
                                                    loadedModule);
            }
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
                KraftLogger.LogError($"DirectCallHandler:Execute has the following error: {sb.ToString()}");
                processingContext.ReturnModel.Status.IsSuccessful = false;
                processingContext.ReturnModel.Status.StatusResults.Add(new StatusResult(){ Message = sb.ToString(), StatusResultType= SysPlugins.Interfaces.Packet.StatusResultEnum.EStatusResult.StatusResultError});
            }
        }

        public IProcessingContextCollection GenerateProcessingContexts(string kraftRequestFlagsKey, ISecurityModel securityModel = null)
        {
            InputModelParameters inputModelParameters = new InputModelParameters();
            inputModelParameters.KraftGlobalConfigurationSettings = _KraftGlobalConfigurationSettings;
            inputModelParameters.ReadAction = _InputModel.ReadAction;
            inputModelParameters.SecurityModel = _InputModel.SecurityModel;
            inputModelParameters.Module = _InputModel.Module;
            inputModelParameters.Nodeset = _InputModel.Nodeset;
            inputModelParameters.Nodepath = _InputModel.Nodepath;
            inputModelParameters.IsWriteOperation = _InputModel.IsWriteOperation;
            inputModelParameters.QueryCollection = _InputModel.QueryCollection;
            inputModelParameters.Data = _InputModel.Data;
            inputModelParameters.LoaderType = ELoaderType.DataLoader;
            inputModelParameters.ServerVariables = new Dictionary<string, object>() {
                { CallTypeConstants.REQUEST_CALL_TYPE, (int)_InputModel.CallType },
                { CallTypeConstants.TASK_KIND, _InputModel.TaskKind },
                { CallTypeConstants.REQUEST_PROCESSOR, "DirectCallHandler" } // Do not use nameof here - this name should remain constant even if the class name changes.
            };

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
            if (loadedNodeSet?.StartNode == null)//Handle errors better and show when a node is addressed but missing.
            {
                sb.Append($"Node: {processingContext.InputModel.Nodepath} from module: {processingContext.InputModel.Module}, nodeset: {processingContext.InputModel.NodeSet} is missing!");
                isError = true;
            }
            return isError;
        }
    }
}
