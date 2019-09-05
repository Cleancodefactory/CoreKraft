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

namespace Ccf.Ck.SysPlugins.Iterators.DataNodes
{
    public class DataNodesImp : IDataIteratorPlugin
    {
        // The older way works either, but I feel more safely to have new DataIteratorContext initialized everytime the new DataNodesImp is requested.
        private readonly DataIteratorContext _dataIteratorContext = new DataIteratorContext();

        #region IDataIteratorPlugin Members
        public async Task<IProcessingContext> ExecuteAsync(
            LoadedNodeSet loadedNodeSet,
            IProcessingContext processingContext,
            IPluginServiceManager pluginServiceManager,
            IPluginAccessor<IDataLoaderPlugin> externalService,
            IPluginAccessor<INodePlugin> customService
            )
        {
            _dataIteratorContext.LoadedNodeSet = loadedNodeSet;
            _dataIteratorContext.ProcessingContext = processingContext;
            _dataIteratorContext.PluginServiceManager = pluginServiceManager;
            _dataIteratorContext.DataLoaderPluginAccessor = externalService;
            _dataIteratorContext.CustomPluginAccessor = customService;
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
            dataIteratorContext.ProcessingContext.ReturnModel.Data = ExecuteReadNode(dataIteratorContext.LoadedNodeSet.StartNode, results, dataIteratorContext);
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
        private object ExecuteReadNode(Node node, IEnumerable<Dictionary<string, object>> parentResult, DataIteratorContext dataIteratorContext)
        {
            #region Preparation of necessary structures
            object returnResult = null;
            // We have to iterate through the parent rows, so we cannot create the results here, but we will do it as soon as possible
            List<Dictionary<string, object>> results = null;

            NodeExecutionContext.Manager execContextManager = new NodeExecutionContext.Manager(dataIteratorContext, node, ACTION_READ);
            #endregion

            // MAIN CYCLE - repeated once per each parent result (for the starting node - it is repeated once)

            foreach (Dictionary<string, object> row in parentResult)
            {
                using (var stackframe = execContextManager.Datastack.Scope(row)) {
                    // 1. Load the main plugin - Data Loader kind
                    IDataLoaderPlugin dataPlugin = null;
                    IPluginsSynchronizeContextScoped contextScoped = null;
                    if (!string.IsNullOrEmpty(node.DataPluginName))
                    {
                        dataPlugin = dataIteratorContext?.DataLoaderPluginAccessor.LoadPlugin(node.DataPluginName);
                        // 2. Update the Node execution context with the actual data for the current loop.
                        //Dictionary<string, object> parentResult = null;
                        contextScoped = dataIteratorContext.DataLoaderPluginAccessor.GetPluginsSynchronizeContextScoped(node.DataPluginName, dataPlugin).Result;
                        if (contextScoped is IContextualBasketConsumer)
                        {
                            var consumer = contextScoped as IContextualBasketConsumer;
                            consumer.InspectBasket(new NodeContextualBasket(execContextManager));
                        }
                    }
                    
                    execContextManager.DataLoaderContextScoped = contextScoped;
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
                        plugins.Execute(node.Read.BeforeNodeActionPlugins, execContextManager.CustomPluginProxy);
                    }
                    #endregion

                    #region (DEPRECATED CODE) Execute BeforeSQL plugins 
                    //pluginExecuteParameters = new CustomPluginExecuteParameters
                    //{
                    //    Phase = "BEFORE_SQL",
                    //    Row = row,
                    //    Results = null,
                    //    Parents = dataIteratorContext.Datastack,
                    //    Path = node.NodeKey, // TODO: This must be the full path to this node.
                    //    Action = OPERATION_READ,
                    //    NodeKey = node.NodeKey,
                    //    NodeParameters = parametersContext.PublicContext

                    //};
                    //pluginExecuteParameters.SqlStatement = GetSqlStatement(node, ACTION_SELECT);
                    //ICustomPluginContext customPluginContext
                    //    = new CustomPluginContext(null, dataIteratorContext.ProcessingContext, contextScoped, node, dataIteratorContext.PluginServiceManager, dataIteratorContext.CustomService);
                    //ICustomPluginProcessor plugins = new CustomPluginProcessor(customPluginContext);

                    //customPluginsResults = plugins?.ExecuteAsync(node.BeforeDataExecutionPlugins, pluginExecuteParameters).Result;
                    #endregion

                    // 4. Data loader execution
                    // 4.1. Execute it (we alreade got the plugin in the beginning - see 1.)
                    // 4.1.1. Update context
                    //execContextManager.Phase = "SQL";
                    // 4.1.2. Execute loader
                    if (execContextManager.LoaderContext.StartNode.Read != null && dataPlugin != null)
                    {
                        dataPlugin.Execute(execContextManager.LoaderPluginProxy);
                    }
                    // The data  loader plugins are responsible to put their results into the REsults collection in the execution context

                    #region DEPRECATED CODE - see above
                    // 4.2. Analyze the result and put it in the official results
                    //if (nodeResult != null && nodeResult is IDictionary) // In order to count as element (record - single record)
                    //{
                    //    results = new List<Dictionary<string, object>>() { ReDictionary(nodeResult) };
                    //}
                    //else if (nodeResult != null && nodeResult is IEnumerable)
                    //{
                    //    // Anything enumerable that is not a dictionary itself we treat as a list/set of results
                    //    //  which shuld be each a IDictionary at least - in order to represent an item.
                    //    results = new List<Dictionary<string, object>>();
                    //    foreach (object el in (nodeResult as IEnumerable))
                    //    {
                    //        if (el is IDictionary)
                    //        {
                    //            // TODO: What if some of the elements is null? Is it a failure or just an empty entry?
                    //            results.Add(ReDictionary(el));
                    //        }
                    //        else
                    //        {
                    //            throw new Exception("The returned result set cotains non-dictionary elements.");
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    throw new ArgumentException($"The return result from a dataplugin has to be either IDictionary<string, object> or List<Dictionary<string, object>> (additional info: node - {node.DataPluginName}");
                    //}
                    #endregion

                    #region DEPRECATED CODE - As with the data loaders the plugins should put their results in the Results themselves
                    // 4.3. Combine the data loader results with those from the custom plugins (if any)
                    // TODO: Probably we should prepend this???
                    // TODO: Use more relaxed type requirements for the results of the plugin (may be?).
                    //if (customPluginsResults != null && customPluginsResults is IEnumerable<Dictionary<string, object>>)
                    //{
                    //    if (results == null)
                    //    {
                    //        results = new List<Dictionary<string, object>>();
                    //    }
                    //    results.AddRange(customPluginsResults as IEnumerable<Dictionary<string, object>>);
                    //}
                    #endregion

                    #region DEPRECATED CODE DataLoaderImp 

                    //object nodeResult = dataPlugin?
                    //    .ExecuteAsync(execContextManager.Context)
                    //    .Result;
                    //// Strict type checking is dangerous - when I saw it first this received REadOnlyDictionary which is not equal to Dictionary.
                    //// if (nodeResult?.GetType() == typeof(Dictionary<string, object>))
                    //if (nodeResult != null && nodeResult is IDictionary) // In order to count as element (record - single record)
                    //{
                    //    results = new List<Dictionary<string, object>>() { ReDictionary(nodeResult) };
                    //    ////results.Add((nodeResult as IDictionary))
                    //    // results.Add(nodeResult as Dictionary<string, object>);
                    //}
                    //// else if (nodeResult?.GetType() == typeof(List<Dictionary<string, object>>))
                    //else if (nodeResult != null && nodeResult is IEnumerable)
                    //{
                    //    results = new List<Dictionary<string, object>>();
                    //    foreach (object el in (nodeResult as IEnumerable))
                    //    {
                    //        if (el is IDictionary)
                    //        {
                    //            // What if some of the elements is null? Is it a failure or just an empty entry?
                    //            results.Add(ReDictionary(el));
                    //        }
                    //        else
                    //        {
                    //            throw new Exception("The returned result set cotains non-dictionary elements.");
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    throw new ArgumentException($"The return result from a dataplugin has to be either IDictionary<string, object> or List<Dictionary<string, object>> (additional info: node - {node.DataPluginName}");
                    //}
                    //// Probably we should prepend this.
                    //// TODO: Use more relaxed type requirements for the results of the plugin (may be?).
                    //if (customPluginsResults != null && customPluginsResults is IEnumerable<Dictionary<string, object>>)
                    //    results.AddRange(customPluginsResults as IEnumerable<Dictionary<string, object>>);
                    #endregion

                    // We have to discuss this - once comitted there is no going back, so we have to be sure this is the best behavior!
                    // 5. Execute custom plugins after SQL
                    // Update context (TODO: are there any more updates neccessary?)
                    //execContextManager.Phase = "AFTER_SQL";
                    // execContextManager.Results = results; // NO NEED
                    // 5.1. After data load (traditionally called AFTER SQL) custom plugins execution.
                    // Now the plugins are required to write to the REsults of the execution context themselves.
                    if(node.Read != null)
                    {
                        plugins?.Execute(node.Read.AfterNodeActionPlugins, execContextManager.CustomPluginProxy);
                    }

                    #region DEPRECATED CODE - see above
                    // 5.2. Accomodate the custom plugin results into the results
                    //if (customPluginsResults != null && customPluginsResults is IEnumerable<Dictionary<string, object>>)
                    //{
                    //    if (results == null)
                    //    {
                    //        results = new List<Dictionary<string, object>>();
                    //    }
                    //    results.AddRange(customPluginsResults as IEnumerable<Dictionary<string, object>>);
                    //}
                    #endregion

                    #region AfterSQL Plugins (Deprecated code)
                    //pluginExecuteParameters = new CustomPluginExecuteParameters
                    //{
                    //    Phase = "AFTER_SQL",
                    //    Row = row,
                    //    Results = null,
                    //    Parents = dataIteratorContext.Datastack,
                    //    Path = node.NodeKey,
                    //    NodeParameters = parametersContext.PublicContext,
                    //    Action = OPERATION_READ,
                    //    NodeKey = node.NodeKey
                    //};
                    //pluginExecuteParameters.SqlStatement = GetSqlStatement(node, ACTION_SELECT);

                    //customPluginsResults = plugins?.ExecuteAsync(node.AfterDataExecutionPlugins, pluginExecuteParameters).Result;

                    //if (customPluginsResults != null && customPluginsResults is IEnumerable<Dictionary<string, object>>)
                    //    results.AddRange(customPluginsResults as IEnumerable<Dictionary<string, object>>);
                    #endregion

                    // 6. Execute the child nodes
                    foreach (Node childNode in node.Children.OrderBy(n => n.ExecutionOrder)) {
                        ExecuteReadNode(childNode, results, dataIteratorContext);
                    }

                    #region Child Nodes (Deprecated code)

                    //if (node.Children != null && node.Children.Count > 0)
                    //{
                    //    IterateChildren(node, results, dataIteratorContext);
                    //}

                    #endregion

                    // 7. Execute custom plugins after children
                    // Update context (TODO: are there any more updates?)
                    //execContextManager.Phase = "AFTER_CHILDREN";
                    // execContextManager.Results = results; // NO NEED - the reference is already there
                    // 7.1. - execute plugin
                    if (node.Read != null)
                    {
                        plugins?.Execute(node.Read.AfterNodeChildrenPlugins, execContextManager.CustomPluginProxy);
                    }
                    // 7.2. Accomodate the custom plugin results into the results
                    #region DEPRECATED CODE
                    //if (customPluginsResults != null && customPluginsResults is IEnumerable<Dictionary<string, object>>)
                    //{
                    //    if (results == null) results = new List<Dictionary<string, object>>();
                    //    results.AddRange(customPluginsResults as IEnumerable<Dictionary<string, object>>);
                    //}
                    #endregion

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

        #region Deprecated code
        //private void IterateChildren(Node node, IEnumerable<Dictionary<string, object>> parentResult, DataIteratorContext dataIteratorContext)
        //{
        //    foreach (Node childNode in node.Children.OrderBy(n => n.ExecutionOrder))
        //    {
        //        ExecuteReadNode(childNode, parentResult, dataIteratorContext);
        //    }
        //}
        #endregion
        #endregion

        #region Write Operation
        #region Deprecated code
        //private object BeginWriteOperation(DataIteratorContext dataIteratorContext)
        //{
        //    object newdata = dataIteratorContext.ProcessingContext.InputModel.Data;
        //    object postData = dataIteratorContext.ProcessingContext.InputModel.Data;
        //    string startNodePath = dataIteratorContext.LoaderContext.StartNode.NodeKey.Trim();
        //    Node startNode = dataIteratorContext.LoaderContext.StartNode;

        //    newdata = ExecuteWriteNode(startNode, postData, startNodePath, dataIteratorContext);
        //    return newdata;
        //}
        #endregion

        private void BeginWriteOperation(DataIteratorContext dataIteratorContext)
        {
            dataIteratorContext.ProcessingContext.ReturnModel.Data =
                ExecuteWriteNode( dataIteratorContext.LoadedNodeSet.StartNode,  
                                  dataIteratorContext.ProcessingContext.InputModel.Data,
                                  dataIteratorContext.LoadedNodeSet.StartNode.NodeKey.Trim(), 
                                  dataIteratorContext);
        }

        private object ExecuteWriteNode(Node node, object dataNode, string nodePath, DataIteratorContext dataIteratorContext)
        {
            #region Preparation of necessary structures
            List<Dictionary<string, object>> currentNode = null;
            object result = dataNode;

            NodeExecutionContext.Manager execContextManager = new NodeExecutionContext.Manager(dataIteratorContext, node, ACTION_WRITE);
            // TODO: ?? execContextManager.Data = dataNode;
            #endregion

            #region Deprecated code
            //CustomPluginExecuteParameters pluginExecuteParameters;

            //ParameterResolverContext parametersContext = new ParameterResolverContext(node)
            //{
            //    ExternalService = dataIteratorContext.ExternalService,
            //    CustomService = dataIteratorContext.CustomService,
            //    PluginServiceManager = dataIteratorContext.PluginServiceManager,
            //    LoaderContext = dataIteratorContext.LoaderContext,
            //    ProcessingContext = dataIteratorContext.ProcessingContext,
            //    Datastack = dataIteratorContext.Datastack,
            //    OverrideAction = dataIteratorContext.OverrideAction
            //};
            
            //NodeExecutionContext.Manager execContextManager = new NodeExecutionContext.Manager(
            //    loaderContext: dataIteratorContext.LoaderContext,
            //    parentResult: null, // Determined only while iterating - the parent result of the individual iteration
            //    pluginServiceManager: dataIteratorContext.PluginServiceManager,
            //    contextScoped: null, // One is obtained for each iteration
            //    currentNode: node,
            //    processingContext: dataIteratorContext.ProcessingContext,
            //    customService: dataIteratorContext.CustomService
            //);

            //object injectResults;
            #endregion

            // 1. Extract the data from more generic forms to list<dictionary> form
            currentNode = ReCodeDataNode(dataNode);
            #region Deprecated code
            //ExtractDataNode(dataNode, ref currentNode, ref result);
            #endregion

            // 1.1. Load the data loader plugin
            IDataLoaderPlugin dataPlugin = null;
            IPluginsSynchronizeContextScoped contextScoped = null;
            if (!string.IsNullOrEmpty(node.DataPluginName))
            {
                dataPlugin = dataIteratorContext?.DataLoaderPluginAccessor.LoadPlugin(node.DataPluginName);

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
                // 3.1 Check for valid state
                if (state == null) // Deprecated:  row.ContainsKey(STATE_PROPERTY_NAME) == false)
                {
                    continue; // Skip non-existent or invalid state - this also skips the children of this node!
                    // throw new ArgumentException("{0} property missing from row. Row processing skipped in ExecuteWriteNode", STATE_PROPERTY_NAME);
                }
                // 3.2 Determine the actual state to use for this iteration (we may have override)
                string operation = GetWriteAction(execContextManager.OverrideAction, state);

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
                if (node.Write != null) {
                    // customPluginsResults = 
                    plugins?.Execute(node.Write.BeforeNodeActionPlugins, execContextManager.CustomPluginProxy);
                }
                #region DEPRECATED CODE Execute BeforeSQL plugins
                //pluginExecuteParameters = new CustomPluginExecuteParameters
                //{
                //    Phase = "BEFORE_SQL",
                //    Row = row,
                //    Results = null,
                //    Parents = dataIteratorContext.Datastack,
                //    Path = node.NodeKey,
                //    NodeParameters = parametersContext.PublicContext,
                //    Action = OPERATION_WRITE,
                //    NodeKey = node.NodeKey
                //};
                //pluginExecuteParameters.SqlStatement = GetSqlStatement(node, action);
                //ICustomPluginContext customPluginContext
                //    = new CustomPluginContext(null, dataIteratorContext.ProcessingContext, contextScoped, node, dataIteratorContext.PluginServiceManager, dataIteratorContext.CustomService);
                //ICustomPluginProcessor plugins = new CustomPluginProcessor();

                //customPluginsResults = plugins?.ExecuteAsync(node.BeforeDataExecutionPlugins, pluginExecuteParameters).Result;

                #endregion

                // 6. Execute children now if the operation is delete
                // Should we remove these?
                #region Reversed Order Processing
                if (operation == OPERATION_DELETE)
                {
                    if (node.Children != null && node.Children.Count > 0) {

                        using (var stackframe = dataIteratorContext.Datastack.Scope(row)) {
                            dataIteratorContext.OverrideAction.Push(OPERATION_DELETE);

                            if (node.Children != null && node.Children.Count > 0) {

                                if (row != null) {
                                    foreach (Node childNode in node.Children) {
                                        string currentNodePath = (!string.IsNullOrEmpty(nodePath))
                                                                     ? nodePath + "." + childNode.NodeKey.Trim()
                                                                     : childNode.NodeKey.Trim();


                                        if (row.ContainsKey(childNode.NodeKey.Trim())) {
                                            // Re-assign the data to the node in case it has to be replaced by the node
                                            //  Not sure we need this, but for now we keep the option open.
                                            // However we should remove the deleted node
                                            object currentDataNode = row[childNode.NodeKey.Trim()];
                                            row[childNode.NodeKey.Trim()] =
                                                ExecuteWriteNode(childNode, currentDataNode, currentNodePath, dataIteratorContext);
                                        }

                                    }
                                }
                            }

                            dataIteratorContext.OverrideAction.Pop();
                        }
                    }
                }
                #endregion

                // 7. Execute main action for this node (the data plugin)
                #region 7. DataLoader processing

                if (operation != OPERATION_UNCHANGED && dataPlugin != null)
                {
                    dataPlugin?.Execute(execContextManager.LoaderPluginProxy);
                }
                #region (DEPRECATED CODE)
                // 7.1. Accomodate the results from before sql plugins
                //if (customPluginsResults != null && customPluginsResults is IEnumerable<Dictionary<string, object>>)
                //{
                //    ApplyResultsToRow(row, customPluginsResults as IEnumerable<Dictionary<string, object>>);
                //}
                #endregion
                #endregion


                #region 8. AfterNodeAction Plugins
                // 8. Switch to after SQL mode (in fact it is also after children if we have delete operation, but we have to catch up step by step.
                //execContextManager.Phase = "AFTER_SQL";

                #region (Deprecated code)
                /*
                pluginExecuteParameters = new CustomPluginExecuteParameters
                {
                    Phase = "AFTER_SQL",
                    Row = row,
                    Results = null,
                    Parents = dataIteratorContext.Datastack,
                    Path = node.NodeKey,
                    NodeParameters = parametersContext.PublicContext,
                    Action = ACTION_WRITE,
                    NodeKey = node.NodeKey
                };
                pluginExecuteParameters.SqlStatement = GetSqlStatement(node, operation);
                */
                #endregion

                #region 8.1. Execute the plugins (after sql)
                if (node.Write != null) {
                    plugins?.Execute(node.Write.AfterNodeActionPlugins, execContextManager.CustomPluginProxy);
                }
                #endregion

                #region (DEPRECATED code)
                // (Deprecated) 8.2. Accomodate the results if any - this is now responsibility of the plugin itself
                //if (customPluginsResults != null && customPluginsResults is IEnumerable<Dictionary<string, object>>)
                //{
                //    ApplyResultsToRow(row, customPluginsResults as IEnumerable<Dictionary<string, object>>);
                //}

                #endregion

                #region NormalOrderProcessing
                // 9. Execute the children now (for non-delete operations).
                // Keeping this code inline makes it a bit easier to follow the processing procedure
                if (operation != OPERATION_DELETE)
                {
                    using (var stackframe = dataIteratorContext.Datastack.Scope(row)) { 
                        if (node.Children != null && node.Children.Count > 0) {

                            if (row != null) {
                                foreach (Node childNode in node.Children) {
                                    string currentNodePath = (!string.IsNullOrEmpty(nodePath))
                                                                 ? nodePath + "." + childNode.NodeKey.Trim()
                                                                 : childNode.NodeKey.Trim();


                                    if (row.ContainsKey(childNode.NodeKey.Trim())) {
                                        object currentDataNode = row[childNode.NodeKey.Trim()];
                                        row[childNode.NodeKey.Trim()] =
                                            ExecuteWriteNode(childNode, currentDataNode, currentNodePath, dataIteratorContext);
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

                #region Deprecated code
                //pluginExecuteParameters = new CustomPluginExecuteParameters
                //{
                //    Phase = "AFTER_CHILDREN",
                //    Row = row,
                //    Results = null,
                //    Parents = dataIteratorContext.Datastack,
                //    Path = node.NodeKey,
                //    Action = ACTION_WRITE,

                //    NodeKey = node.NodeKey,
                //    NodeParameters = parametersContext.PublicContext
                //};
                //pluginExecuteParameters.SqlStatement = GetSqlStatement(node, operation);
                #endregion deprecated code
                // 11. Execute the plugins for after children
                if (node.Write != null)
                {
                    plugins?.Execute(node.Write.AfterNodeChildrenPlugins, execContextManager.CustomPluginProxy);
                }
                #region DEPRECATED CODE
                // 11.1. Accomodate the results from the plugins.
                //if (customPluginsResults != null && customPluginsResults is IEnumerable<Dictionary<string, object>>)
                //    ApplyResultsToRow(row, customPluginsResults as IEnumerable<Dictionary<string, object>>);
                #endregion
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
                    if(!currentResult.ContainsKey(key))
                    {
                        currentResult.Add(key, currentNode[i][key]);
                    }
                }
            }

            return new ReadOnlyDictionary<string, object>(currentResult);
        }

        private object IterateChildren(Node node, string nodePath, object dataNode, DataIteratorContext dataIteratorContext)
        {
            var dictionaryDataNode = dataNode as Dictionary<string, object>;
            if (dictionaryDataNode != null)
            {
                foreach (Node childNode in node.Children)
                {
                    string currentNodePath = (!string.IsNullOrEmpty(nodePath))
                                                 ? nodePath + "." + childNode.NodeKey.Trim()
                                                 : childNode.NodeKey.Trim();
                    object currentDataNode = dictionaryDataNode[childNode.NodeKey.Trim()];

                    dictionaryDataNode[childNode.NodeKey.Trim()] =
                        ExecuteWriteNode(childNode, currentDataNode, currentNodePath, dataIteratorContext);
                }
            }
            return dataNode;
        }
        
        #endregion

        #region Helpers
        private List<Dictionary<string,object>> ReCodeDataNode(object dataNode) {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            if (dataNode is IDictionary) { // single item
                var item = ReDictionary(dataNode);
                if (item != null) result.Add(item);
                // TODO: Should we do something when the item is not converted?
            } else if (dataNode is IEnumerable) {
                foreach (object item in (dataNode as IEnumerable)) {
                    // Each item must be dictionary, because a list in list is not supported
                    if (item is IDictionary) {
                        var _item = ReDictionary(item);
                        if (_item != null) result.Add(_item);
                    }
                }
            }

            return result;
        }

        private void ExtractDataNode(object dataNode, ref List<Dictionary<string, object>> currentNode, ref object result)
        {
            if (dataNode is IDictionary<string, object>)
            {
                Dictionary<string, object> dictionaryParams;
                ReadOnlyDictionary<string, object> readOnlyDictionaryParams = dataNode as ReadOnlyDictionary<string, object>;
                if (readOnlyDictionaryParams != null)
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
                    var a = el as Dictionary<string, object>;
                    if (a != null) currentNode.Add(a);
                }
            }
        }

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
        // TODO: IS this still needed here? We moved this kind of activity to the plugins
        private void ApplyResultsToRow(Dictionary<string, object> row, IEnumerable<Dictionary<string, object>> customPluginResults)
        {
            if (row != null && customPluginResults != null)
            {
                foreach (var customPluginResult in customPluginResults)
                {
                    if (customPluginResult != null)
                    {
                        foreach (var kvp in customPluginResult)
                        {
                            row[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
        }

        private string GetSqlStatement(Node node, string action)
        {
            string result = string.Empty;
            switch (action)
            {
                case OPERATION_SELECT:
                    if ((node.Read.Select != null) && node.Read.Select.HasStatement()) result = node.Read.Select.Query;
                    break;
                case OPERATION_INSERT:
                    if ((node.Write.Insert != null) && node.Write.Insert.HasStatement()) result = node.Write.Insert.Query;
                    break;
                case OPERATION_DELETE:
                    if ((node.Write.Delete != null) && node.Write.Delete.HasStatement()) result = node.Write.Delete.Query;
                    break;
                case OPERATION_UPDATE:
                    if ((node.Write.Update != null) && node.Write.Update.HasStatement()) result = node.Write.Update.Query;
                    break;

            }
            return result;
        }
        #endregion
    }
}
