using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Iterators.DataNodes.CustomPluginsExecution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Libs.Logging;

namespace Ccf.Ck.SysPlugins.Iterators.DataNodes
{
    public class DataNodesImp : IDataIteratorPlugin
    {
        // The older way works either, but I feel more safely to have new DataIteratorContext initialized everytime the new DataNodesImp is requested.
        private readonly DataIteratorContext _dataIteratorContext = new DataIteratorContext();

        #region IDataIteratorPlugin Members
        public async Task<IProcessingContext> ExecuteAsync(
            LoadedNodeSet loaderContext,
            IProcessingContext processingContext,
            IPluginServiceManager pluginServiceManager,
            IPluginAccessor<IDataLoaderPlugin> dataLoaderAccessor,
            IPluginAccessor<INodePlugin> customPluginAccessor = null
            )
        {
            _dataIteratorContext.LoadedNodeSet = loaderContext;
            _dataIteratorContext.ProcessingContext = processingContext;
            _dataIteratorContext.PluginServiceManager = pluginServiceManager;
            _dataIteratorContext.DataLoaderPluginAccessor = dataLoaderAccessor;
            _dataIteratorContext.CustomPluginAccessor = customPluginAccessor;
            _dataIteratorContext.CheckNulls();

            // dataIteratorContext already has a reference to processingContext.ReturrnModel.Data, 
            // that's why previous implementation with assigning the retturn result from Begin(Read\Write)Operation to it is obsolete.

            if (processingContext.InputModel.IsWriteOperation == false)
            {
                BeginReadOperation(_dataIteratorContext);
            }
            else if (processingContext.InputModel.IsWriteOperation == true)
            {
                BeginWriteOperation(_dataIteratorContext);
            }

            return await Task.FromResult(processingContext);
        }
        public async Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()
        {
            return await Task.FromResult(new DataNodesSynchronizeContextScopedImp());
        }

        #endregion

        #region ReadOperation

        private void BeginReadOperation(DataIteratorContext dataIteratorContext)
        {
            //  Trace.WithContext(dataIteratorContext.ProcessingContext.TraceId).Log("Nodeset READ operation starting- nodeset: {0}, modekey: {1}",dataIteratorContext.LoadedNodeSet.StartNode.NodeSet.Name, dataIteratorContext.LoadedNodeSet.StartNode.NodeKey);
            var results = new List<Dictionary<string, object>>() { new Dictionary<string, object>() { } };
            EMetaInfoFlags infoFlag = dataIteratorContext.ProcessingContext.InputModel.KraftGlobalConfigurationSettings.GeneralSettings.MetaLoggingEnumFlag;
            dataIteratorContext.ProcessingContext.ReturnModel.ExecutionMeta = new MetaRoot(infoFlag); // TODO: Choose the flags from config
            object result = ExecuteReadNode(dataIteratorContext.LoadedNodeSet.StartNode, results, dataIteratorContext, dataIteratorContext.ProcessingContext.ReturnModel.ExecutionMeta);
            dataIteratorContext.ProcessingContext.ReturnModel.ExecutionMeta.SetFinished();
            if (dataIteratorContext.BailOut) return;
            dataIteratorContext.ProcessingContext.ReturnModel.Data = result;
            
        }

        /// <summary>
        /// Executes a read action on a node. The procedure is:
        /// - Single and multiple results are processed the same way, but when single result is required (not is list) all but the first one are removed
        /// - For each result all child nodes are executed (i.e. this routine is executed on each result and they create/update a property corresponding
        ///     to the key under which they are configured. This is achieved by calling the function with the node argument that gives it the key under which it
        ///     should attach its result(s).
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parentResult"></param>
        /// <param name="dataIteratorContext"></param>
        /// <returns></returns>
        private object ExecuteReadNode(Node node, IEnumerable<Dictionary<string, object>> parentResult, DataIteratorContext dataIteratorContext, IIteratorMeta metaStore)
        {
            // Check if we have anything to do here
            if (node.Read == null) return null; // No read instructions - nothing to read
            
            #region Preparation of necessary structures
            var metaNode = metaStore.Child(node.NodeKey);

            bool _bailOut() {
                return dataIteratorContext.BailOut;
            }
            object returnResult = null;
            EReadAction readAction = dataIteratorContext.ProcessingContext.InputModel.ReadAction; // Select by default
            
            // We have to iterate through the parent rows, so we cannot create the results here, but we will do it as soon as possible
            List<Dictionary<string, object>> results = null;

            NodeExecutionContext.Manager execContextManager = new NodeExecutionContext.Manager(dataIteratorContext, node, ACTION_READ, metaNode);
            #endregion

            #region Main loader plugin creation
            // 1. Load the main plugin - Data Loader kind
            // This context will be the same for all the parent produced rows over which we execute a child node
            IDataLoaderPlugin dataPlugin = null;
            IPluginsSynchronizeContextScoped contextScoped = null;
            string pluginName = null;
            if (readAction == EReadAction.Select) {

                if (!string.IsNullOrEmpty(node.DataPluginName)) {
                    pluginName = node.DataPluginName;
                }
            } else if (readAction == EReadAction.New) {
                if (!string.IsNullOrEmpty(node?.Read?.New?.Plugin)) {
                    pluginName = node.Read.New.Plugin;
                }
                else
                {
                    if (!string.IsNullOrEmpty(node?.Read?.New?.Query))
                    {
                        //There is no plugin but query, most probably misconfigured plugin name
                        KraftLogger.LogWarning($"Query content for node {node.NodeSet.Name}/{node.NodeKey}: {Environment.NewLine}{node?.Read?.New?.Query}{Environment.NewLine}won't be executed because NEW-PLUGIN not configured.");
                    }
                }
            } else {
                throw new Exception("Unsupported read action");
            }
            if (pluginName != null) {
                dataPlugin = dataIteratorContext.DataLoaderPluginAccessor.LoadPlugin(pluginName);
                // 2. Update the Node execution context with the actual data for the current loop.
                //Dictionary<string, object> parentResult = null;
                contextScoped = dataIteratorContext.DataLoaderPluginAccessor.GetPluginsSynchronizeContextScoped(pluginName, dataPlugin).Result;
                if (contextScoped is IContextualBasketConsumer) {
                    var consumer = contextScoped as IContextualBasketConsumer;
                    consumer.InspectBasket(new NodeContextualBasket(execContextManager));
                }
            }
            // Set the scoped context to the node execution context
            execContextManager.DataLoaderContextScoped = contextScoped;
            #endregion



            // MAIN CYCLE - repeated once per each parent result (for the starting node - it is repeated once)

            foreach (Dictionary<string, object> row in parentResult)
            {
                using (var stackframe = execContextManager.Datastack.Scope(row))
                {
                   
                    // execContextManager.ParentResult = row; // Wrong
                    //execContextManager.Phase = "BEFORE_SQL"; // I think 'Phase' is a relic, couldn't find any usage.
                    execContextManager.Row = row; // This is needed by the resolvers, but is not visible to plugins!

                    // 2.1 The reulsts - from this part on we need it
                    results = new List<Dictionary<string, object>>();
                    // 2.1.1 Put it into the execution context so everything executed from this point on can put results into the collection
                    execContextManager.Results = results;

                    // 3. Execute custom plugins (injects) before main work
                    // 3.1. Prepare plugins - through the processor
                    // object customPluginsResults = new Dictionary<string, object>();
                    ICustomPluginProcessor plugins = new CustomPluginProcessor();

                    // 3.3 The procedure has been changed, now the plugins are responsible to attach results in the Results collection
                    #region 3.3 execute BEFORE SQL plugins over the results
                    if (node.Read != null)
                    {
                        plugins.Execute(node.Read.BeforeNodeActionPlugins, execContextManager.CustomPluginProxy, _bailOut);
                        if (_bailOut()) return null;
                    }
                    #endregion

                    // 4. Data loader execution
                    // 4.1. Execute it (we already got the plugin in the beginning - see 1.)

                    // 4.1.2. Execute loader
                    if (execContextManager.LoaderContext.StartNode.Read != null && dataPlugin != null)
                    {
                        dataPlugin.Execute(execContextManager.LoaderPluginProxy);
                        if (_bailOut()) return null;
                    }
                    // The data  loader plugins are responsible to put their results into the Results collection in the execution context
                    // We have to discuss this - once comitted there is no going back, so we have to be sure this is the best behavior!
                    // 5. Execute custom plugins after SQL
                    // Update context (TODO: are there any more updates neccessary?)
                    //execContextManager.Phase = "AFTER_SQL";
                    // execContextManager.Results = results; // NO NEED
                    // 5.1. After data load (traditionally called AFTER SQL) custom plugins execution.
                    // Now the plugins are required to write to the REsults of the execution context themselves.
                    if (node.Read != null)
                    {
                        plugins?.Execute(node.Read.AfterNodeActionPlugins, execContextManager.CustomPluginProxy, _bailOut);
                        if (_bailOut()) return null;
                    }

                    // 6. Execute the child nodes
                    foreach (Node childNode in node.Children.OrderForReadExecution(readAction))
                    {
                        ExecuteReadNode(childNode, results, dataIteratorContext, metaNode);
                        if (_bailOut()) return null;
                    }


                    // 7. Execute custom plugins after children
                    // 7.1. - execute plugin
                    if (node.Read != null)
                    {
                        plugins?.Execute(node.Read.AfterNodeChildrenPlugins, execContextManager.CustomPluginProxy, _bailOut);
                        if (_bailOut()) return null;
                    }
                    // 7.2. Accomodate the custom plugin results into the results
                    #region AfterChildren Plugins (Deprecated code)
                    //pluginExecuteParameters = new CustomPluginExecuteParameters
                    //{
                    //    Phase = "AFTER_CHILDREN",
                    //    Row = row,
                    //    Results = null,
                    //    Parents = dataIteratorContext.Datastack,
                    //    Path = node.NodeKey,
                    //    NodeParameters = parametersContext.PublicContext,
                    //    Action = OPERATION_READ,
                    //    NodeKey = node.NodeKey
                    //};
                    //pluginExecuteParameters.SqlStatement = GetSqlStatement(node, ACTION_SELECT);

                    //customPluginsResults = plugins?.ExecuteAsync(node.AfterChildreDataExecutionPlugins, pluginExecuteParameters).Result;

                    //if (customPluginsResults != null && customPluginsResults is IEnumerable<Dictionary<string, object>>)
                    //    results.AddRange(customPluginsResults as IEnumerable<Dictionary<string, object>>);
                    #endregion
                }
                // 8. Final repacking
                #region Final Re-Packing
                if (node.IsList)
                {
                    row.Add(node.NodeKey.Trim(), results);
                    returnResult = results;
                }
                else if (results != null && results.Count > 0)
                {
                    row.Add(node.NodeKey.Trim(), results[0]);
                    returnResult = results[0];
                }
                #endregion
            }

            return returnResult;
        }
        /// <summary>
        /// Basically converts IDictionary to IDictionary(string, object) to allow more different types to act as a data item.
        /// </summary>
        /// <param name="nodeResult">An item from the node results</param>
        /// <returns></returns>
        private static Dictionary<string, object> ReDictionary(object nodeResult)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            foreach (DictionaryEntry de in (nodeResult as IDictionary))
            {
                if (de.Key == null || !(de.Key is string))
                {
                    throw new Exception("dataPlugin returned data containing key(s) which are null or not a string.");
                }
                dict.Add(de.Key as string, de.Value);
            }
            return dict;
        }

        #endregion

        #region Write Operation

        private void BeginWriteOperation(DataIteratorContext dataIteratorContext)
        {
            EMetaInfoFlags infoFlag = dataIteratorContext.ProcessingContext.InputModel.KraftGlobalConfigurationSettings.GeneralSettings.MetaLoggingEnumFlag;
            dataIteratorContext.ProcessingContext.ReturnModel.ExecutionMeta = new MetaRoot(infoFlag); 
            object result = ExecuteWriteNode(dataIteratorContext.LoadedNodeSet.StartNode,
                                  dataIteratorContext.ProcessingContext.InputModel.Data,
                                  dataIteratorContext.LoadedNodeSet.StartNode.NodeKey.Trim(),
                                  dataIteratorContext,
                                  dataIteratorContext.ProcessingContext.ReturnModel.ExecutionMeta);
            dataIteratorContext.ProcessingContext.ReturnModel.ExecutionMeta.SetFinished();
            if (dataIteratorContext.BailOut) return;
            dataIteratorContext.ProcessingContext.ReturnModel.Data = result;
            

        }

        private object ExecuteWriteNode(Node node, object dataNode, string nodePath, DataIteratorContext dataIteratorContext, IIteratorMeta metaStore)
        {
            #region Preparation of necessary structures
            var metaNode = metaStore.Child(node.NodeKey);
            bool _bailOut() {
                return dataIteratorContext.BailOut;
            }
            List<Dictionary<string, object>> currentNode = null;
            object result = dataNode;

            NodeExecutionContext.Manager execContextManager = new NodeExecutionContext.Manager(dataIteratorContext, node, ACTION_WRITE, metaNode);
            // TODO: ?? execContextManager.Data = dataNode;
            #endregion

            // 1. Extract the data from more generic forms to list<dictionary> form
            currentNode = ReCodeDataNode(dataNode);

            // 1.1. Load the data loader plugin
            IDataLoaderPlugin dataPlugin = null;
            IPluginsSynchronizeContextScoped contextScoped = null;
            if (!string.IsNullOrEmpty(node.DataPluginName))
            {
                dataPlugin = dataIteratorContext.DataLoaderPluginAccessor.LoadPlugin(node.DataPluginName);

                //Dictionary<string, object> parentResult = null;
                contextScoped = dataIteratorContext.DataLoaderPluginAccessor.GetPluginsSynchronizeContextScoped(node.DataPluginName, dataPlugin).Result;
                if (contextScoped is IContextualBasketConsumer)
                {
                    var consumer = contextScoped as IContextualBasketConsumer;
                    consumer.InspectBasket(new NodeContextualBasket(execContextManager));
                }
            }

            // 2. Main cycle.
            //  Split by ordering the items by non-deleted and deleted state for easier processing
            foreach (Dictionary<string, object> row in
                  currentNode.OrderBy(n => (n.ContainsKey(STATE_PROPERTY_NAME) &&
                        (string)n[STATE_PROPERTY_NAME] == STATE_PROPERTY_DELETE
                    ) ? 0
                      : 1))
            {
                //TODO the current implementation is not enforcing the IsList property!!!
                if (row == null) continue;

                // 3 Get the state
                var state = execContextManager.DataState.GetDataState(row); //row[STATE_PROPERTY_NAME] as string;
                metaNode.GetVolatileInfo().DataState = state;
                // 3.1 Check for valid state
                if (state == null) // Deprecated:  row.ContainsKey(STATE_PROPERTY_NAME) == false)
                {
                    continue; // Skip non-existent or invalid state - this also skips the children of this node!
                    // throw new ArgumentException("{0} property missing from row. Row processing skipped in ExecuteWriteNode", STATE_PROPERTY_NAME);
                }
                // 3.2 Determine the actual state to use for this iteration (we may have override)
                string operation = GetWriteAction(execContextManager.OverrideAction, state);
                metaNode.GetVolatileInfo().Operation = operation;

                // The action is the actual state we assume!

                #region Fill the values for current iteration in the exec context
                execContextManager.DataLoaderContextScoped = contextScoped; // Change it for this iteration
                // execContextManager.ParentResult = row; // Wrong
                //execContextManager.Phase = "BEFORE_SQL";
                execContextManager.Row = row;
                execContextManager.Operation = operation;
                #endregion

                // 5. Execute plugins before SQL
                // DEPRECATED :Results from plugins - we will reuse this in the next phases too.
                // Now the plugins have to store data themselves in Row
                // object customPluginsResults = null;
                ICustomPluginProcessor plugins = new CustomPluginProcessor();

                // 5.1. Do execute
                if (node.Write != null)
                {
                    // customPluginsResults = 
                    plugins?.Execute(node.Write.BeforeNodeActionPlugins, execContextManager.CustomPluginProxy, _bailOut);
                    if (_bailOut()) return null;
                }

                // 6. Execute children now if the operation is delete
                // 6.1 Split the children between pre and post - used in non-delete for now
                List<Node> preChildren = null;
                List<Node> postChildren = null;
                // Should we remove these?
                #region Reversed Order Processing
                if (operation == OPERATION_DELETE)
                {
                    if (node.Children != null && node.Children.Count > 0)
                    {
                        using (var stackframe = dataIteratorContext.Datastack.Scope(row))
                        {
                            dataIteratorContext.OverrideAction.Push(OPERATION_DELETE);

                            if (node.Children != null && node.Children.Count > 0)
                            {

                                if (row != null)
                                {
                                    foreach (Node childNode in node.Children)
                                    {
                                        string currentNodePath = (!string.IsNullOrEmpty(nodePath))
                                                                     ? nodePath + "." + childNode.NodeKey.Trim()
                                                                     : childNode.NodeKey.Trim();


                                        if (row.ContainsKey(childNode.NodeKey.Trim()))
                                        {
                                            // Re-assign the data to the node in case it has to be replaced by the node
                                            //  Not sure we need this, but for now we keep the option open.
                                            // However we should remove the deleted node
                                            object currentDataNode = row[childNode.NodeKey.Trim()];
                                            row[childNode.NodeKey.Trim()] =
                                                ExecuteWriteNode(childNode, currentDataNode, currentNodePath, dataIteratorContext, metaNode);
                                            if (_bailOut()) break;
                                        }

                                    }
                                }
                            }

                            dataIteratorContext.OverrideAction.Pop();
                            if (_bailOut()) return null;
                        }
                    }
                }
                else // For now we do not touch the delete, but we will
                {
                    preChildren = node.Children.ForWriteExecution(operation, true);
                    postChildren = node.Children.ForWriteExecution(operation, false);

                    using (var stackframe = dataIteratorContext.Datastack.Scope(row)) {
                        if (preChildren != null && preChildren.Count > 0) {

                            if (row != null) {
                                foreach (Node childNode in preChildren) {
                                    string currentNodePath = (!string.IsNullOrEmpty(nodePath))
                                                                 ? nodePath + "." + childNode.NodeKey.Trim()
                                                                 : childNode.NodeKey.Trim();


                                    if (row.ContainsKey(childNode.NodeKey.Trim())) {
                                        object currentDataNode = row[childNode.NodeKey.Trim()];
                                        row[childNode.NodeKey.Trim()] =
                                            ExecuteWriteNode(childNode, currentDataNode, currentNodePath, dataIteratorContext, metaNode);
                                        if (_bailOut()) return null;
                                    }

                                }
                            }
                        }
                    }
                }
                #endregion

                // 7. Execute main action for this node (the data plugin)
                #region 7. DataLoader processing

                if (operation != OPERATION_UNCHANGED && dataPlugin != null)
                {
                    dataPlugin?.Execute(execContextManager.LoaderPluginProxy);
                    if (_bailOut()) return null;
                }
                #endregion


                #region 8. AfterNodeAction Plugins
                // 8. Switch to after SQL mode (in fact it is also after children if we have delete operation, but we have to catch up step by step.
                //execContextManager.Phase = "AFTER_SQL";

                #region 8.1. Execute the plugins (after sql)
                if (node.Write != null)
                {
                    plugins?.Execute(node.Write.AfterNodeActionPlugins, execContextManager.CustomPluginProxy, _bailOut );
                    if (_bailOut()) return null;
                }
                #endregion

                #region NormalOrderProcessing
                // 9. Execute the children now (for non-delete operations).
                // Keeping this code inline makes it a bit easier to follow the processing procedure
                // It is not a problem that the postChildren is pre-calculated - it depends on the existence of the nodes.
                if (operation != OPERATION_DELETE)
                {
                    using (var stackframe = dataIteratorContext.Datastack.Scope(row))
                    {
                        if (postChildren != null && postChildren.Count > 0)
                        {

                            if (row != null)
                            {
                                foreach (Node childNode in postChildren)
                                {
                                    string currentNodePath = (!string.IsNullOrEmpty(nodePath))
                                                                 ? nodePath + "." + childNode.NodeKey.Trim()
                                                                 : childNode.NodeKey.Trim();


                                    if (row.ContainsKey(childNode.NodeKey.Trim()))
                                    {
                                        object currentDataNode = row[childNode.NodeKey.Trim()];
                                        row[childNode.NodeKey.Trim()] =
                                            ExecuteWriteNode(childNode, currentDataNode, currentNodePath, dataIteratorContext, metaNode);
                                        if (_bailOut()) return null;
                                    }

                                }
                            }
                        }
                    }
                }
                #endregion

                #endregion

                #region AfterNodeChildren Plugins
                // 10. Switch to after children mode
                //execContextManager.Phase = "AFTER_SQL";

                // 11. Execute the plugins for after children
                if (node.Write != null)
                {
                    plugins?.Execute(node.Write.AfterNodeChildrenPlugins, execContextManager.CustomPluginProxy, _bailOut);
                    if (_bailOut()) return null;
                }

                #endregion
            }

            // 12. We have to clean up deleted rows completely
            #region Final Re-Packing
            for (int i = currentNode.Count - 1; i >= 0; i--)
            {
                Dictionary<string, object> row = currentNode[i];
                if (row.ContainsKey(STATE_PROPERTY_NAME) && (row[STATE_PROPERTY_NAME] as string) == STATE_PROPERTY_DELETE)
                {
                    currentNode.RemoveAt(i); // Remove deleted records
                }
            }
            #endregion
            // TODO: needs better implementation. This is temporary fix only
            // TODO: Do we really need this at all??? result is kind of empty at the moment
            // result = UpdateResults(currentNode, result);

            //return result;\
            // This depends on the re-assignment.
            if (dataNode is IDictionary<string, object>)
            {
                if (currentNode.Count > 0) return currentNode[0];
                return null; // TODO: May be empty object? Can we be here at all if anything was wrong?
            }
            else
            {
                return currentNode;
            }
        }

        private object UpdateResults(List<Dictionary<string, object>> currentNode, object result)
        {
            if (!(result is ReadOnlyDictionary<string, object>))
            {
                return result;
            }
            Dictionary<string, object> currentResult = (result as ReadOnlyDictionary<string, object>).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            for (int i = 0; i < currentNode.Count; i++)
            {
                foreach (var key in currentNode[i].Keys)
                {
                    if (!currentResult.ContainsKey(key))
                    {
                        currentResult.Add(key, currentNode[i][key]);
                    }
                }
            }

            return new ReadOnlyDictionary<string, object>(currentResult);
        }

        #endregion

        #region Helpers
        private List<Dictionary<string, object>> ReCodeDataNode(object dataNode)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (dataNode is IDictionary)
            { // single item
                var item = ReDictionary(dataNode);
                if (item != null) result.Add(item);
                // TODO: Should we do something when the item is not converted?
            }
            else if (dataNode is IEnumerable)
            {
                foreach (object item in (dataNode as IEnumerable))
                {
                    // Each item must be dictionary, because a list in list is not supported
                    if (item is IDictionary)
                    {
                        var _item = ReDictionary(item);
                        if (_item != null) result.Add(_item);
                    }
                }
            }

            return result;
        }

        /* Kept fpr reference "what we did with varying data in the input"
        private void ExtractDataNode(object dataNode, ref List<Dictionary<string, object>> currentNode, ref object result)
        {
            if (dataNode is IDictionary<string, object>)
            {
                Dictionary<string, object> dictionaryParams;
                if (dataNode is ReadOnlyDictionary<string, object> readOnlyDictionaryParams)
                {
                    dictionaryParams = readOnlyDictionaryParams.ToDictionary(kv => kv.Key, kv => kv.Value);
                }
                else
                {
                    dictionaryParams = dataNode as Dictionary<string, object>;
                }
                currentNode = new List<Dictionary<string, object>> { dictionaryParams };
            }
            else if (dataNode is List<Dictionary<string, object>>)
            {
                currentNode = dataNode as List<Dictionary<string, object>>; // Use the list itself
            }
            else if (dataNode is Array)
            {
                currentNode = new List<Dictionary<string, object>>(); // Repack the array into list
                result = currentNode;
                foreach (object el in dataNode as IEnumerable)
                {
                    if (el is Dictionary<string, object> a)
                    {
                        currentNode.Add(a);
                    }
                }
            }
        }
        */

        private string GetWriteAction(Stack<string> overrideAction, string state)
        {
            if (overrideAction != null && overrideAction.Count > 0)
            {
                return overrideAction.Peek();
            }
            else
            {
                switch (state)
                {
                    case STATE_PROPERTY_UPDATE:
                        return OPERATION_UPDATE;
                    case STATE_PROPERTY_INSERT:
                        return OPERATION_INSERT;
                    case STATE_PROPERTY_DELETE:
                        return OPERATION_DELETE;
                    default:
                        return OPERATION_UNCHANGED; // do not process it - assume unchanged
                }
            }
        }
        
        #endregion
    }
}
