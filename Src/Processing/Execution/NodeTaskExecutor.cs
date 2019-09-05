using System;
using Ccf.Ck.Models.Packet;
using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Utilities.Profiling;
using GenericUtilities = Ccf.Ck.Utilities.Generic.Utilities;
using Ccf.Ck.Libs.Logging;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces.NodeExecution;
using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;
using System.Threading.Tasks;

namespace Ccf.Ck.Processing.Execution
{
    public class NodeTaskExecutor : INodeTaskExecutor
    {
        private ITransactionScopeContext _TransactionScope;
        private KraftModuleConfigurationSettings _KraftModuleConfigurationSettings;
        private bool _CollectiveCall;

        public NodeTaskExecutor(ITransactionScopeContext transactionScope, KraftModuleConfigurationSettings moduleSettings)
        {
            _TransactionScope = transactionScope ?? throw new ArgumentNullException(nameof(transactionScope));
            _KraftModuleConfigurationSettings = moduleSettings;
        }

        public void ExecuteNodeData(
            LoadedNodeSet loaderContextDefinition,
            IProcessingContext processingContext,
            IPluginAccessor<IDataLoaderPlugin> dataLoaderAccessor,
            IPluginAccessor<INodePlugin> pluginAccessor)
        {
            if (!_CollectiveCall)
            {
                //Check for null values
                GenericUtilities.CheckNullOrEmpty(loaderContextDefinition, true);
                GenericUtilities.CheckNullOrEmpty(processingContext, true);
                GenericUtilities.CheckNullOrEmpty(_TransactionScope.PluginServiceManager, true);
            }
            try
            {
                if (processingContext.InputModel.LoaderType.HasFlag(ELoaderType.DataLoader))
                {
                    using (KraftProfiler.Current.Step("Execution time loading data: "))
                    {
                        if (loaderContextDefinition.StartNode != null && loaderContextDefinition.StartNode.HasValidDataSection(processingContext.InputModel.IsWriteOperation))
                        {
                            IDataIteratorPlugin dataIteratorPlugin = Utilities.GetPlugin<IDataIteratorPlugin>(_KraftModuleConfigurationSettings.NodeSetSettings.SourceLoaderMapping.NodesDataIterator.NodesDataIteratorConf.Name, _TransactionScope.DependencyInjectionContainer, _KraftModuleConfigurationSettings, ELoaderType.DataLoader, true);
                            GenericUtilities.CheckNullOrEmpty(dataIteratorPlugin, true);
                            IProcessingContext r = dataIteratorPlugin.ExecuteAsync(loaderContextDefinition, processingContext, _TransactionScope.PluginServiceManager, dataLoaderAccessor, pluginAccessor).Result;

                        }
                    }
                }
                if (!_CollectiveCall)
                {
                    _TransactionScope.CommitTransactions();
                }
            }
            catch (Exception ex)
            {
                processingContext.ReturnModel.Status.IsSuccessful = false;
                processingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { StatusResultType = EStatusResult.StatusResultError, Message = ex.Message });
                if (!_CollectiveCall)
                {
                    _TransactionScope.RollbackTransactions();
                }
                KraftLogger.LogError(ex.Message, ex);
            }
        }

        public void ExecuteNodeView(
            LoadedNodeSet loaderContextDefinition,
            IProcessingContext processingContext)
        {
            if (!_CollectiveCall)
            {
                //Check for null values
                GenericUtilities.CheckNullOrEmpty(loaderContextDefinition, true);
                GenericUtilities.CheckNullOrEmpty(processingContext, true);
                GenericUtilities.CheckNullOrEmpty(_TransactionScope.PluginServiceManager, true);
            }

            try
            {
                using (KraftProfiler.Current.Step("Execution time loading views: "))
                {
                    if (!string.IsNullOrEmpty(processingContext.InputModel.BindingKey))
                    {
                        View view = loaderContextDefinition.StartNode.Views.Find(v => v.BindingKey.Equals(processingContext.InputModel.BindingKey, StringComparison.OrdinalIgnoreCase));
                        ExecuteNodeViewPrivate(loaderContextDefinition, processingContext, view);
                    }
                    else
                    {
                        foreach (View view in loaderContextDefinition.StartNode.Views)
                        {
                            ExecuteNodeViewPrivate(loaderContextDefinition, processingContext, view);
                        }
                    }                    
                }
                if (!_CollectiveCall)
                {
                    _TransactionScope.CommitTransactions();
                }
            }
            catch (Exception ex)
            {
                processingContext.ReturnModel.Status.IsSuccessful = false;
                processingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { StatusResultType = EStatusResult.StatusResultError, Message = ex.Message });
                if (!_CollectiveCall)
                {
                    _TransactionScope.RollbackTransactions();
                }
                KraftLogger.LogError(ex.Message, ex);
            }
        }

        private void ExecuteNodeViewPrivate(LoadedNodeSet loaderContextDefinition, IProcessingContext processingContext, View view)
        {
            if (view != null)
            {
                ISystemPlugin systemPlugin = Utilities.GetPlugin<ISystemPlugin>(view.SystemPluginName, _TransactionScope.DependencyInjectionContainer, _KraftModuleConfigurationSettings, ELoaderType.ViewLoader);
                GenericUtilities.CheckNullOrEmpty(systemPlugin, true);
                IPluginsSynchronizeContextScoped synchronizeContextScoped = _TransactionScope.GetSynchronizeContextScopedAsync(view.SystemPluginName, ELoaderType.ViewLoader, _KraftModuleConfigurationSettings, systemPlugin).Result;
                GenericUtilities.CheckNullOrEmpty(synchronizeContextScoped, true);
                systemPlugin.ExecuteAsync(loaderContextDefinition, processingContext, _TransactionScope.PluginServiceManager, synchronizeContextScoped, view);
            }
        }

        public void Execute(
            LoadedNodeSet loaderContextDefinition, 
            IProcessingContext processingContext, 
            IPluginAccessor<IDataLoaderPlugin> dataLoaderAccessor,
            IPluginAccessor<INodePlugin> pluginAccessor)
        {
            _CollectiveCall = true;
            //Check for null values
            GenericUtilities.CheckNullOrEmpty(loaderContextDefinition, true);
            GenericUtilities.CheckNullOrEmpty(processingContext, true);
            GenericUtilities.CheckNullOrEmpty(_TransactionScope.PluginServiceManager, true);

            try
            {
                if (processingContext.InputModel.LoaderType.HasFlag(ELoaderType.DataLoader))
                {
                    ExecuteNodeData(loaderContextDefinition, processingContext, dataLoaderAccessor, pluginAccessor);
                }

                if (processingContext.InputModel.LoaderType.HasFlag(ELoaderType.ViewLoader))
                {
                    ExecuteNodeView(loaderContextDefinition, processingContext);
                }
                
                //if (processingContext.InputModel.LoaderType.HasFlag(ELoaderType.LookupLoader))
                //{
                //    if (loaderContextDefinition.StartNode != null && loaderContextDefinition.StartNode.HasLookup())
                //    {
                //        using (KraftProfiler.Current.Step("Execution time loading lookups: "))
                //        {
                //            foreach (Lookup lookup in loaderContextDefinition.StartNode.Lookups)
                //            {
                //                ISystemPlugin systemPlugin = Utilities.GetPlugin<ISystemPlugin>(lookup.SystemPluginName, _TransactionScope.DependencyInjectionContainer, _KraftModuleConfigurationSettings, ELoaderType.LookupLoader);
                //                GenericUtilities.CheckNullOrEmpty(systemPlugin, true);
                //                IPluginsSynchronizeContextScoped synchronizeContextScoped = await _TransactionScope.GetSynchronizeContextScopedAsync(lookup.SystemPluginName, ELoaderType.LookupLoader, _KraftModuleConfigurationSettings, systemPlugin);
                //                GenericUtilities.CheckNullOrEmpty(synchronizeContextScoped, true);
                //                await systemPlugin.ExecuteAsync(loaderContextDefinition, processingContext, _TransactionScope.PluginServiceManager, synchronizeContextScoped, lookup);
                //            }
                //        }
                //    }
                //}

                _TransactionScope.CommitTransactions();
            }
            catch (Exception ex)
            {
                processingContext.ReturnModel.Status.IsSuccessful = false;
                processingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { StatusResultType = EStatusResult.StatusResultError, Message = ex.Message });
                _TransactionScope.RollbackTransactions();
                KraftLogger.LogError(ex.Message, ex);
            }
            finally
            {
                _CollectiveCall = false;
            }
        }
    }
}
