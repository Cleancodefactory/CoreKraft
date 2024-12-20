using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Packet;
using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces.NodeExecution;
using Ccf.Ck.Utilities.Profiling;
using Microsoft.Data.Sqlite;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;
using GenericUtilities = Ccf.Ck.Utilities.Generic.Utilities;

namespace Ccf.Ck.Processing.Execution
{
    public class NodeTaskExecutor : INodeTaskExecutor
    {
        private readonly ITransactionScopeContext _TransactionScope;
        private readonly KraftModuleConfigurationSettings _KraftModuleConfigurationSettings;

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
            try
            {
                //Check for null values
                GenericUtilities.CheckNullOrEmpty(loaderContextDefinition, true);
                GenericUtilities.CheckNullOrEmpty(processingContext, true);
                GenericUtilities.CheckNullOrEmpty(_TransactionScope.PluginServiceManager, true);
                if (processingContext.InputModel.LoaderType.HasFlag(ELoaderType.DataLoader))
                {
                    using (KraftProfiler.Current.Step("Execution time loading data: "))
                    {
                        if (loaderContextDefinition.StartNode != null /*&& loaderContextDefinition.StartNode.HasValidDataSection(processingContext.InputModel.IsWriteOperation)*/)
                        {
                            IDataIteratorPlugin dataIteratorPlugin = Utilities.GetPlugin<IDataIteratorPlugin>(_KraftModuleConfigurationSettings.NodeSetSettings.SourceLoaderMapping.NodesDataIterator.NodesDataIteratorConf.Name, _TransactionScope.DependencyInjectionContainer, _KraftModuleConfigurationSettings, ELoaderType.DataLoader, true);
                            GenericUtilities.CheckNullOrEmpty(dataIteratorPlugin, true);
                            dataIteratorPlugin.Execute(loaderContextDefinition, processingContext, _TransactionScope.PluginServiceManager, dataLoaderAccessor, pluginAccessor);
                        }
                    }
                }
                _TransactionScope.CommitTransactions();
            }
            catch
            {
                _TransactionScope.RollbackTransactions();
                throw;
            }
        }

        public void ExecuteNodeView(
            LoadedNodeSet loaderContextDefinition,
            IProcessingContext processingContext)
        {
            try
            {
                //Check for null values
                GenericUtilities.CheckNullOrEmpty(loaderContextDefinition, true);
                GenericUtilities.CheckNullOrEmpty(processingContext, true);
                GenericUtilities.CheckNullOrEmpty(_TransactionScope.PluginServiceManager, true);

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
                    _TransactionScope.CommitTransactions();
                }
            }
            catch
            {
                _TransactionScope.RollbackTransactions();
                throw;
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
                systemPlugin.Execute(loaderContextDefinition, processingContext, _TransactionScope.PluginServiceManager, synchronizeContextScoped, view);
            }
        }

        public void Execute(
            LoadedNodeSet loaderContextDefinition,
            IProcessingContext processingContext,
            IPluginAccessor<IDataLoaderPlugin> dataLoaderAccessor,
            IPluginAccessor<INodePlugin> pluginAccessor)
        {
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
                //TODO if in mode trace pipeline, collect all the nodes with params and values which were called and where exception occurred.
                processingContext.ReturnModel.Status.IsSuccessful = false;
                processingContext.ReturnModel.Status.StatusResults.Add(new StatusResult { StatusResultType = EStatusResult.StatusResultError, Message = ex.Message });
                _TransactionScope.RollbackTransactions();
                if (ex is AggregateException aggregateException)
                {
                    foreach (Exception exception in aggregateException.InnerExceptions)
                    {
                        if (exception is ThreadInterruptedException)
                        {
                            throw exception;
                        }
                    }
                }

                StringBuilder errMsg = new StringBuilder(1000);
                errMsg.AppendLine();
                errMsg.AppendLine($"Module: {processingContext.InputModel.Module}");
                errMsg.AppendLine($"Nodeset: {loaderContextDefinition.NodeSet.Name}");
                errMsg.AppendLine($"Startnodekey: {loaderContextDefinition.StartNode.NodeKey}");
                if (loaderContextDefinition.StartNode.CollectedReadParameters != null)
                {
                    errMsg.AppendLine($"CollectedReadParameters: {string.Join(",", loaderContextDefinition.StartNode.CollectedReadParameters.Select(p => p.Name))}");
                }
                if (loaderContextDefinition.StartNode.CollectedWriteParameters != null)
                {
                    errMsg.AppendLine($"CollectedWriteParameters: {string.Join(",", loaderContextDefinition.StartNode.CollectedWriteParameters.Select(p => p.Name))}");
                }
                errMsg.AppendLine($"DataPluginName: {loaderContextDefinition.StartNode.DataPluginName}");
                errMsg.AppendLine($"RequireAuthentication: {loaderContextDefinition.StartNode.RequireAuthentication}");
                errMsg.AppendLine($"Error.Message: {ex.Message}");
                if (ex != null && ex.InnerException is SqliteException sqliteException)
                {
                    if (sqliteException.SqliteErrorCode != 19 || !sqliteException.Message.Contains("Cannot set the update counter to -1."))
                    {
                        KraftLogger.LogError(errMsg.ToString(), ex);
                    }
                }
                else
                {
                    KraftLogger.LogError(errMsg.ToString(), ex);
                }
            }
        }
    }
}
