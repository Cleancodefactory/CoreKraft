using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Ccf.Ck.SysPlugins.Utilities;
using Ccf.Ck.Libs.ResolverExpression;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Utilities.Generic;
using System.ComponentModel;
using Ccf.Ck.SysPlugins.Support.ParameterExpression.Managers;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Interfaces.NodeExecution;

namespace Ccf.Ck.SysPlugins.Iterators.DataNodes
{
    /// <summary>
    /// CONCEPT:
    ///     - The execution of an individual node in a nodeset is backed by a context holding all the referenes and values related to the execution.
    ///     
    ///     - Part of the values and references are updated in the process, but most are not - they have longer lifecycles than the node execution.
    ///     
    ///     - The manager nested class is the actual constructor of the context and gives the iterator ways to change the refs and values that need
    ///         changing.
    ///         
    ///     - The context itself provides everything that can be read-only at any certain moment as read-only, but is still not consumed directly.
    ///     
    ///     - The manager creates 3 proxies (in the current versions) which provide access to some parts of the context - one proxy for custom
    ///         plugins, data loaders and resolverss.
    /// 
    /// 
    /// A context that lives from the start to the end of an individual node execution.
    /// As the nodes call subnode executions as part of their processing, this means several such conexts may be alive at the same time.
    /// CONSIDER:
    ///     1. We can link them, put them in stack/list or make them cross-accessible in some other way.
    ///     2. We can refactor the entry methods of the plugins involved to depend on this context and link
    ///         the other contexts and parameters they use here instead passing them directly or through other contexts.
    ///         
    /// PROBLEMS:
    ///     We have to be abstract and generic to avoid cyclic references, especially if we want to do the above.
    /// </summary>
    public class NodeExecutionContext : INodeExecutionContext, IDataStateHelperProvider<IDictionary<string, object>>
    {

        #region Modes of operation
        private bool _cacheParameters = true;
        /// <summary>
        /// Not currently settable and fully supported yet - for future use
        /// When set disables throwing of exceptions during compilation, evaluation and related code.
        /// </summary>
        private readonly bool _noEvaluationExceptions = false;
        #endregion

        #region Construction
        private NodeExecutionContext(DataIteratorContext dic, Node currentNode, string action)
        {
            LoadedNodeSet = dic.LoadedNodeSet;
            PluginServiceManager = dic.PluginServiceManager;
            CurrentNode = currentNode;
            ProcessingContext = dic.ProcessingContext;
            CustomService = dic.CustomPluginAccessor;
            Path = currentNode.NodeKey; // TODO: Must be the full path
            Action = action;
            NodeKey = currentNode.NodeKey;
            Datastack = dic.Datastack;
            OverrideAction = dic.OverrideAction;
            BailOut = () => { dic.BailOut = true; };
        }
        #endregion

        #region Proxy contexts instances
        /// <summary>
        /// A context proxy passed to resolvers from compiled expressions
        /// </summary>
        public IParameterResolverContext ParameterResolverProxy { get; private set; }
        /// <summary>
        /// A context proxy passed to the custom plugins - it is either read or write context
        /// </summary>
        public CustomPluginContext CustomPluginProxy { get; private set; }
        
        /// <summary>
        /// A context proxy passed to the Data loader plugins
        /// </summary>
        public LoaderPluginContext LoaderPluginProxy { get; private set; }

        /// <summary>
        /// A context proxy for the pre-plugin custom plugins.
        /// </summary>
        public CustomPluginContext CustomPluginPreNodeProxy { get; private set; }
        #endregion

        /// <summary>
        /// The manager is held by the node iterator and through it the control over its content is complete.
        /// </summary>
        public class Manager : IDataStateHelperProvider<IDictionary<string, object>>
        {
            private NodeExecutionContext Context { get; set; }

            internal Manager(DataIteratorContext dic, Node currentNode, string action, IExecutionMeta metaNode)
            {
                Context = new NodeExecutionContext(dic, currentNode, action);
                Context.ParameterResolverProxy = new ParameterResolverContext(Context);
                Context.CustomPluginProxy = (action == ACTION_READ) ? (new CustomPluginReadContext(Context) as CustomPluginContext): (new CustomPluginWriteContext(Context) as CustomPluginContext);
                Context.LoaderPluginProxy = (action == ACTION_READ) ?new LoaderPluginReadContext(Context) as LoaderPluginContext:new LoaderPluginWriteContext(Context) as LoaderPluginContext;
                Context.CustomPluginPreNodeProxy = new CustomPluginPreNodeContext(Context);
                Context.MetaNode = metaNode;
            }

            public IDataStateHelper<IDictionary<string, object>> DataState => DataStateUtility.Instance;

            public IPluginsSynchronizeContextScoped DataLoaderContextScoped {
                get => Context.DataLoaderContextScoped;
                set => Context.DataLoaderContextScoped = value;
            }
            public IPluginsSynchronizeContextScoped OwnContextScoped {
                get => Context.OwnContextScoped;
                set => Context.OwnContextScoped = value;
            }
            public Dictionary<string, object> ParentResult
            {
                get => Context.ParentResult;
            }
            public Dictionary<string, object> Row
            {
                get => Context.Row;
                set => Context.Row = value;
            }
            public List<Dictionary<string, object>> Results
            {
                get => Context.Results;
                set => Context.Results = value;
            }
            [EditorBrowsable(EditorBrowsableState.Never)]
            public string Path
            {
                get => Context.Path;
                set => Context.Path = value;
            }
            //public string Phase
            //{
            //    get => Context.Phase;
            //    set => Context.Phase = value;
            //}
            [EditorBrowsable(EditorBrowsableState.Never)]
            public string Action
            {
                get => Context.Action;
                set => Context.Action = value;
            }
            [EditorBrowsable(EditorBrowsableState.Never)]
            public string Operation
            {
                get => Context.Operation;
                set => Context.Operation = value;
            }
            [EditorBrowsable(EditorBrowsableState.Never)]
            public string NodeKey
            {
                get => Context.NodeKey;
                set => Context.NodeKey = value;
            }
            [EditorBrowsable(EditorBrowsableState.Never)]
            public Node CurrentNode
            {
                get => Context.CurrentNode;
                set => Context.CurrentNode = value;
            }
            /// <summary>
            /// Start node and root of the pacage
            /// </summary>
            [EditorBrowsable(EditorBrowsableState.Never)]
            public LoadedNodeSet LoaderContext
            {
                get => Context.LoadedNodeSet;
                set => Context.LoadedNodeSet = value;
            }
            [EditorBrowsable(EditorBrowsableState.Never)]
            public IProcessingContext ProcessingContext
            {
                get => Context.ProcessingContext;
                set => Context.ProcessingContext = value;
            }
            [EditorBrowsable(EditorBrowsableState.Never)]
            public IPluginServiceManager PluginServiceManager
            {
                get => Context.PluginServiceManager;
                set => Context.PluginServiceManager = value;
            }
            [EditorBrowsable(EditorBrowsableState.Never)]
            public IPluginAccessor<INodePlugin> CustomService
            {
                get => Context.CustomService;
                set => Context.CustomService = value;
            }
            [EditorBrowsable(EditorBrowsableState.Never)]
            public ListStack<Dictionary<string, object>> Datastack
            {
                get => Context.Datastack;
                set => Context.Datastack = value;
            }
            public object Data
            {
                get => Context.Data;
                set => Context.Data = value;
            }
            public Stack<string> OverrideAction
            {
                get => Context.OverrideAction;
                set => Context.OverrideAction = value;
            }
            public NodePluginPhase ExecutionPhase {
                get => Context.ExecutionPhase;
                set => Context.ExecutionPhase = value;
            }
            // Proxies access
            public IParameterResolverContext ParameterResolverProxy => Context.ParameterResolverProxy;
            /// <summary>
            /// A context proxy passed to the custom plugins
            /// </summary>
            public CustomPluginContext CustomPluginProxy => Context.CustomPluginProxy;
            /// <summary>
            /// A context proxy passed to the Data loader plugins
            /// </summary>
            public LoaderPluginContext LoaderPluginProxy => Context.LoaderPluginProxy;
            /// <summary>
            /// A context proxy for the pre-plugin custom plugins.
            /// </summary>
            public CustomPluginContext CustomPluginPreNodeProxy => Context.CustomPluginPreNodeProxy;
            public LoaderPluginReadContext LoaderPluginPrepareProxy() {
                return new LoaderPluginReadContext(Context);
            }
        }

        // ---=== CONTEXT ITEMS ===---

        /// <summary>
        /// The root of the generated/saved data tree.
        /// (not sure if we really need this - it is debatable)
        /// </summary>
        public object Data { get; private set; }

        #region Parameters and resolvers
        /// <summary>
        /// Extract the parameters definition from the Node configuration
        /// </summary>
        private List<Parameter> Parameters
        {
            get
            {
                return CollectParameters(CurrentNode);
            }
        }

        private List<Parameter> CollectParameters(Node currentNode)
        {
            if (currentNode != null)
            {
                if (Action.Equals(ACTION_READ))
                {
                    if (currentNode.CollectedReadParameters != null)
                    {
                        return currentNode.CollectedReadParameters;
                    }
                }
                else if (Action.Equals(ACTION_WRITE))
                {
                    if (currentNode.CollectedWriteParameters != null)
                    {
                        return currentNode.CollectedWriteParameters;
                    }
                }
                
                Dictionary<string, Parameter> collectedParametersDictionary = new Dictionary<string, Parameter>(10);
                //1. check the root for parameters
                collectedParametersDictionary = currentNode.NodeSet.Root.Parameters?.ToDictionary(p => p.Name);
                if (collectedParametersDictionary == null)
                {
                    collectedParametersDictionary = new Dictionary<string, Parameter>(10);
                }
                //2. check the current node for parameters
                foreach (Parameter parameter in currentNode.Parameters ?? new List<Parameter>())
                {
                    collectedParametersDictionary[parameter.Name] = parameter;
                }
                //3. check the operations Read or Write for parameters
                if (Action.Equals(ACTION_READ) && currentNode.Read != null)
                {
                    foreach (Parameter parameter in currentNode.Read.Parameters ?? new List<Parameter>())
                    {
                        collectedParametersDictionary[parameter.Name] = parameter;
                    }
                    //Merge all parameters and return them
                    currentNode.CollectedReadParameters = collectedParametersDictionary.Values.ToList();
                    return currentNode.CollectedReadParameters;
                }
                else if (Action.Equals(ACTION_WRITE) && currentNode.Write != null)
                {
                    foreach (Parameter parameter in currentNode.Write.Parameters ?? new List<Parameter>())
                    {
                        collectedParametersDictionary[parameter.Name] = parameter;
                    }
                    //Merge all parameters and return them
                    currentNode.CollectedWriteParameters = collectedParametersDictionary.Values.ToList();
                    return currentNode.CollectedWriteParameters;
                }                
            }
            throw new NullReferenceException("CurrentNode is null");
        }
        /// <summary>
        /// When called the evaluator will look here and if the expression is not yet compilled it will find it in Parameters and compile it here
        /// </summary>
        private readonly Dictionary<string, ResolverRunner<ParameterResolverValue, IParameterResolverContext>> CompiledParameterExpressions = new Dictionary<string, ResolverRunner<ParameterResolverValue, IParameterResolverContext>>();

        private readonly Dictionary<string, ParameterResolverValue> CompiledParameterExpressionsCache = new Dictionary<string, ParameterResolverValue>();


        #endregion

        #region Properties changing during execution (cycle < node_exec)
        public Dictionary<string, object> Row { get; private set; }
        public List<Dictionary<string, object>> Results { get; set; }
        //public string Phase { get; private set; }
        public Dictionary<string, object> ParentResult {
            get {
                return Datastack.Top();
            }
        }
        public Stack<string> OverrideAction { get; private set; }

        public Action BailOut { get; private set; }
        public string Operation { get; private set; }
        /// <summary>
        /// Own scoped context - 
        ///     - in custom plugin- its own
        ///     - in DataLoader this is the same as DataLoaderContextScoped
        /// </summary>
        public IPluginsSynchronizeContextScoped OwnContextScoped { get; private set; }

        public IPluginsSynchronizeContextScoped DataLoaderContextScoped { get; private set; }

        #endregion

        #region Cycle >= node_exec)
        public string Path { get; private set; }
        public string Action { get; private set; }
        public string NodeKey { get; private set; }
        public EReadAction ReadAction => this.ProcessingContext.InputModel.ReadAction;
        public IExecutionMeta MetaNode { get; private set; }

        public NodePluginPhase ExecutionPhase { get; private set; }

        /// <summary>
        /// Node configuration
        /// </summary>
        public Node CurrentNode { get; private set; }
        /// <summary>
        /// Start node and root of the pacage
        /// </summary>
        public LoadedNodeSet LoadedNodeSet { get; private set; }
        public IProcessingContext ProcessingContext { get; private set; }
        public ListStack<Dictionary<string, object>> Datastack { get; private set; }
        public IPluginServiceManager PluginServiceManager { get; private set; }

        public IPluginAccessor<INodePlugin> CustomService { get; private set; }
        #endregion


        // ---=== CONTEXT ITEMS END ===---

        #region Parameter expressions invocation
        private const string DEFAULT_PARAMETER_NAME = "$default";
        private Parameter GetParameterByName(string name)
        {
            if (Parameters != null)
            {
                return Parameters.FirstOrDefault(p => String.Compare(p.Name, name, false) == 0);
            }
            return null;
        }

        #region values cache 
        /* 
         * Currently these are not even exposed and the cache is always on, but we want to be able to
         * try some more advanced behaviors in future.
         * 
         * Current process:
         *  1. Node start - context and cache are created
         *  2. PreSQL custom plugins are executed.
         *      2.1. They may use parameters and thus cause them to be calculated and put into the cache
         *  3. Main pahse (SQL) - DataLoader may consume parameters and cause them to go into the cache.
         *  4. PostSQL plugins 
         *  5. Execute children. For now this is just "waiting" them to complete
         *  6. PostChildren plugins.
         *  
         *  *.1. Evaluaing each parameter may indirectly cause others to be evaluated and cached.
         *  
         *  The consequence of this procedure is that each parameter is executed only once and the cached value is used further.
         *  The evaluation may happen by request of any plugin and thus the constant behavior, obvios for DbCommand expands over the whole
         *  node and over all participants of its execution.
         *  
         *  This seems to be the best default behavior, but certain doors are left open (you will see some code that deals with that - 
         *  it is there to keep these options available) for future expansion that will enable some
         *  expressions (for some parameters) to be reexecuted each time they are used. 
         * 
        */


        /// <summary>
        /// Clears the cache and turns caching on or off
        /// </summary>
        /// <param name="enableAndClear"></param>
        public void CacheParameters(bool enableAndClear)
        {
            CompiledParameterExpressionsCache.Clear();
            _cacheParameters = enableAndClear;
        }
        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void ClearCache()
        {
            CompiledParameterExpressionsCache.Clear();
        }
        #endregion values cache

        private ResolverRunner<ParameterResolverValue, IParameterResolverContext> GetParameterRunner(string expressionName, out bool isdefault, out bool neverCache, bool noDefault = false, string initialExpressionName = null)
        {
            if (string.IsNullOrWhiteSpace(expressionName)) throw new ArgumentNullException($"{nameof(expressionName)} canot be null");
            neverCache = false; // TODO: Add a property to the Parameter to specify this
            isdefault = false;
            if (CompiledParameterExpressions.ContainsKey(expressionName)) return CompiledParameterExpressions[expressionName];
            // Not compiled - compile it
            var param = GetParameterByName(expressionName);
            bool dummy;
            if (param != null)
            {
                if (!string.IsNullOrWhiteSpace(param.Expression))
                {
                    // just try to compile it
                    var runner = ParameterResolversManager.Instance.Compiler.CompileResolverExpression(param.Expression);
                    if (runner == null) throw new Exception("Not a valid expression.");
                    if (!runner.IsValid) throw new Exception($"Error while compiling expression: {runner.ErrorText}");
                    // If we are here - it is valid runner
                    CompiledParameterExpressions[expressionName] = runner;
                    return runner;
                }
                else
                { // Use the runner for the expression of the default parameter
                    if (param.Name == DEFAULT_PARAMETER_NAME)
                    {
                        // We have to stop - this is the default parameter, but it does not have an expression
                        throw new Exception($"The default parameter {DEFAULT_PARAMETER_NAME} if specified must have a valid resolver expression specified in {nameof(param.Expression)}");
                    }
                    // We have to use the default here

                    var defrunner = GetParameterRunner(DEFAULT_PARAMETER_NAME, out dummy, out neverCache, false, expressionName);
                    isdefault = true;
                    // The check here is paranoic - exceptions should be already thrown if the ocndition fails.
                    if (defrunner != null && defrunner.IsValid) return defrunner;
                    throw new Exception($"Expression: '{expressionName}' or '{initialExpressionName}' can't be resolved. Check capital letter because the search is case sensitive."); // should never happen
                }
            }
            else
            {
                if (!noDefault)
                {
                    // Parameter not found - get the default runner
                    var defrunner = GetParameterRunner(DEFAULT_PARAMETER_NAME, out dummy, out neverCache, true, expressionName);
                    isdefault = true;
                    // The check here is paranoic - exceptions should be already thrown if the ocndition fails.
                    if (defrunner != null && defrunner.IsValid) return defrunner;
                }
                throw new Exception($"Expression: '{expressionName}' or '{initialExpressionName}' can't be resolved. Check capital letter because the search is case sensitive."); // should never happen
            }
        }
        public ParameterResolverValue Evaluate(string expressionName, IList<ParameterResolverValue> oldargs = null)
        {
            // Must not be null - we need to know what are we resolving
            if (string.IsNullOrWhiteSpace(expressionName))
            {
                throw new ArgumentNullException(expressionName);
            }
            ParameterResolverValue result = new ParameterResolverValue(null, EResolverValueType.Invalid);
            bool neverCache = false;
            ResolverRunner<ParameterResolverValue, IParameterResolverContext> runner = null;
            try
            {

                // Check the cache
                //if (false)
                //{
                //    if (CompiledParameterExpressionsCache.ContainsKey(expressionName)) return CompiledParameterExpressionsCache[expressionName];
                //}
                // No cache or not in cache - get the runner
                bool isdefault;
                runner = GetParameterRunner(expressionName, out isdefault, out neverCache);
            }
            catch (Exception ex)
            {
                if (!_noEvaluationExceptions)
                {
                    throw new Exception($"Error while resolving parameter {expressionName}",ex);
                }
            }
            if (runner != null)
            {
                result = runner.EvaluateScalar(
                    ctx: ParameterResolverProxy,
                    ex: out Exception ex,
                    name: new ParameterResolverValue(expressionName, EValueDataType.Text),
                    callerargs: oldargs);
                if (ex == null)
                {
                    //if (ex != null) throw ex;
                    if ((_cacheParameters && !neverCache) && expressionName != DEFAULT_PARAMETER_NAME)
                    {
                        CompiledParameterExpressionsCache[expressionName] = result;
                    }
                    return result;
                }
                else
                {
                    if (_noEvaluationExceptions)
                    {
                        return result;
                    }
                    else
                    {
                        throw new Exception($"Error resolving parameter {expressionName}", ex);
                    }
                }

            }
            else
            {
                if (_noEvaluationExceptions)
                {
                    return result;
                }
                else
                {
                    throw new Exception($"The expression {expressionName} cannot compile for unknown reasons.");
                }
            }
        }

        #region Deprecated code
        //public ParameterResolverValue Evaluate(string expressionName, IList<ParameterResolverValue> oldargs = null) {
        //    if (string.IsNullOrWhiteSpace(expressionName)) throw new ArgumentNullException(expressionName);
        //    ParameterResolverValue result = new ParameterResolverValue(null, EResolverValueType.Invalid);
        //    ResolverRunner<ParameterResolverValue, IParameterResolverContext> runner = null;
        //    if (CompiledParameterExpressions.ContainsKey(expressionName)) {
        //        // From cache if possible
        //        if (_cacheParameters && CompiledParameterExpressionsCache.ContainsKey(expressionName)) return CompiledParameterExpressionsCache[expressionName];
        //        // If not evaluate and put in cache if requested
        //        Exception ex = null;
        //        result = runner.EvaluateScalar(
        //            ctx: ParameterResolverProxy,
        //            ex: out ex,
        //            name: new ParameterResolverValue(expressionName, EValueDataType.Text),
        //            callerargs: oldargs);
        //        if (ex != null) throw ex;
        //        if (_cacheParameters && expressionName != DEFAULT_PARAMETER_NAME) CompiledParameterExpressionsCache[expressionName] = result;
        //        return result;
        //    } else { // Not compiled
        //        Parameter param = GetParameterByName(expressionName);
        //        if (param == null) {
        //            // use default
        //            param = GetParameterByName(DEFAULT_PARAMETER_NAME);
        //        }
        //        if (param != null) {

        //        } else {
        //            throw new Exception("Cannot find parameter " + expressionName + " and no 'default' parameter is defined");
        //        }

        //    }
        //    if (!CompiledParameterExpressions.ContainsKey(expressionName)) {
        //        Parameter param = GetParameterByName(expressionName);
        //        if (param != null && !string.IsNullOrWhiteSpace(param.Expression)) {
        //            runner = ParameterResolversManager.Instance.Compiler.CompileResolverExpression(param.Expression);

        //            if (runner.IsValid) {
        //                CompiledParameterExpressions[expressionName] = runner;
        //            } else {
        //                throw new Exception(runner.ErrorText);
        //            }
        //        } else {
        //            // TODO: (DEFAULT) See if we would want a default expression instead of throwing an error.
        //            throw new Exception("Cannot find parameter " + expressionName);
        //        }
        //    } else {
        //        runner = CompiledParameterExpressions[expressionName];
        //    }
        //    if (runner != null) // it is checked for validity when first compiled and not even registered if it is not, so existence is enough here
        //    {
        //        Exception ex = null;
        //        result = runner.EvaluateScalar(
        //            ctx: ParameterResolverProxy,
        //            ex: out ex,
        //            name: new ParameterResolverValue(expressionName, EValueDataType.Text),
        //            callerargs: oldargs);
        //    }
        //    return result;
        //}
        #endregion Deprecated code
        #endregion


        #region Data state helpers
        public IDataStateHelper<IDictionary<string, object>> DataState => DataStateUtility.Instance;
        #endregion

        #region Context proxies
        public class NodeExecutionContextProxy : IDataStateHelperProvider<IDictionary<string, object>>, IActionHelpers
        {
            protected NodeExecutionContext Context { get; private set; }

            #region Current action helpers
            /// <summary>
            /// Returns the currently performed action
            /// </summary>
            /// <returns></returns>
            public ActionBase PerformedAction() {
                if (Context.Action == ACTION_READ) {
                    if (Context.CurrentNode.Read != null) {
                        return Context.ReadAction switch {
                            EReadAction.Select => Context.CurrentNode.Read?.Select,
                            EReadAction.New => Context.CurrentNode.Read?.New,
                            _ => null
                        };
                    }
                } else if (Context.Action == ACTION_WRITE) {
                    if (Context.CurrentNode.Write != null) {
                        return Context.Operation switch {
                            OPERATION_INSERT => Context.CurrentNode.Write?.Insert,
                            OPERATION_UPDATE => Context.CurrentNode.Write?.Update,
                            OPERATION_DELETE => Context.CurrentNode.Write?.Delete,
                            _ => null
                        };
                    }
                }
                return null;
            }
            /// <summary>
            /// Executes a delegate for the current action if it is of type A. Use as many of these as needed to execute code for different action cases
            /// </summary>
            /// <typeparam name="A"></typeparam>
            /// <param name="action"></param>
            /// <returns></returns>
            public bool ForCurrentAction<A>(Action<A> action) where A: ActionBase {
                ActionBase actionBase = PerformedAction();
                if (actionBase == null) return false;
                if (typeof(A) == actionBase.GetType()) {
                    action(actionBase as A);
                    return true;
                }
                return false;
            }
            #endregion
            #region General action helpers
            public A GetAction<A>() where A: ActionBase {

                if (typeof(A) == typeof(Select)) return Context.CurrentNode?.Read?.Select as A;
                else if (typeof(A) == typeof(New)) return Context.CurrentNode?.Read?.New as A;
                else if (typeof(A) == typeof(Insert)) return Context.CurrentNode?.Write?.Insert as A;
                else if (typeof(A) == typeof(Update)) return Context.CurrentNode?.Write?.Update as A;
                else if (typeof(A) == typeof(Delete)) return Context.CurrentNode?.Write?.Delete as A;
                return null;
            }
            /// <summary>
            /// Executes a delegate for action A if it exists, does nothing otherwise
            /// </summary>
            /// <typeparam name="A">One of the ActionBase types</typeparam>
            /// <param name="action">Delegate to execute</param>
            /// <returns>Boolean indicating if execution was performed or not.</returns>
            public bool OverAction<A>(Action<A> action) where A: ActionBase {
                A a = GetAction<A>();
                if (a == null) return false;
                action(a);
                return true;
            }
            #endregion

            public IDataStateHelper<IDictionary<string, object>> DataState => DataStateUtility.Instance;

            public NodeExecutionContextProxy(NodeExecutionContext ctx)
            {
                Context = ctx;
            }
            public void BailOut() {
                Context.BailOut();
            }
            public IExecutionMeta NodeMeta => Context.MetaNode;
        }
        
        /// <summary>
        /// This is the proxy presented to the resolvers
        /// </summary>
        public class ParameterResolverContext : NodeExecutionContextProxy, IParameterResolverContext
        {
            public ParameterResolverContext(NodeExecutionContext nodectx) : base(nodectx) { }

            public IPluginAccessor<INodePlugin> CustomService => Context.CustomService;

            public List<Dictionary<string, object>> Datastack => Context.Datastack;

            public LoadedNodeSet LoadedNodeSet => Context.LoadedNodeSet;

            public Node CurrentNode => Context.CurrentNode;

            public Stack<string> OverrideAction => Context.OverrideAction;

            public IPluginServiceManager PluginServiceManager => Context.PluginServiceManager;

            public IProcessingContext ProcessingContext => Context.ProcessingContext;

            //public string Phase => Context.Phase;
            public Dictionary<string, object> Row => Context.Row;
            public List<Dictionary<string, object>> Results => Context.Results;

            public string Path => Context.Path;

            public string Action => Context.Action;

            public string Operation => Context.Operation;

            public string NodeKey => Context.NodeKey;

            public ParameterResolverValue Evaluate(string expressionName, IList<ParameterResolverValue> oldargs = null)
            {
                return Context.Evaluate(expressionName, oldargs);
            }
        }
        /// <summary>
        /// The proxy presented to the custom plugins
        /// </summary>
        public class CustomPluginContext : NodeExecutionContextProxy, INodePluginContext
        {
            public CustomPluginContext(NodeExecutionContext nodectx) : base(nodectx) { }
            public IProcessingContext ProcessingContext => Context.ProcessingContext;
            //public IPluginsSynchronizeContextScoped ContextScoped => Context.ContextScoped;
            public IPluginServiceManager PluginServiceManager => Context.PluginServiceManager;

            public IPluginAccessor<INodePlugin> CustomPluginAccessor => Context.CustomService;
            public Node CurrentNode => Context.CurrentNode;

            public List<Dictionary<string, object>> Datastack => Context.Datastack;
            public string Path => Context.Path;
            //public string Phase => Context.Phase;
            // public string RowState { get; set; } // Used only when Action = store to indicate the state of the this.Row (this is a helper you have Api.Set/GetRowState as alternative ways to deal with this)
            public string Action => Context.Action;
            public string Operation => Context.Operation;
            public string NodeKey => Context.NodeKey;
            public object Data => Context.Data;
            public NodePluginPhase ExecutionPhase => Context.ExecutionPhase;

            public IPluginsSynchronizeContextScoped OwnContextScoped { get; set; }

            public IPluginsSynchronizeContextScoped DataLoaderContextScoped=> Context.DataLoaderContextScoped; 

            public ParameterResolverValue Evaluate(string expressionName, IList<ParameterResolverValue> oldargs = null)
            {
                return Context.Evaluate(expressionName, oldargs);
            }
            public CustomPlugin CustomPlugin { get; set; }
        }

        public class CustomPluginReadContext : CustomPluginContext, INodePluginReadContext {
            public CustomPluginReadContext(NodeExecutionContext nodectx) : base(nodectx) {}
            public List<Dictionary<string, object>> Results => Context.Results;

        }
        public class CustomPluginWriteContext : CustomPluginContext, INodePluginWriteContext {
            public CustomPluginWriteContext(NodeExecutionContext nodectx) : base(nodectx) { }

            public Dictionary<string, object> Row => Context.Row;

        }

        public class CustomPluginPreNodeContext : CustomPluginContext, INodePluginPreNodeContext, INodePluginReadContext, INodePluginWriteContext {
            public CustomPluginPreNodeContext(NodeExecutionContext nodectx) : base(nodectx) { }

            public List<Dictionary<string, object>> Results => Context.Results;

            public Dictionary<string, object> Row => null;

        }
        /// <summary>
        /// The proxy presented to the loader plugins
        /// </summary>
        public class LoaderPluginContext : NodeExecutionContextProxy, IDataLoaderContext
        { // DONE - has to use IDataLoaderContext!!!
            public LoaderPluginContext(NodeExecutionContext nodectx) : base(nodectx) { }

            public IPluginsSynchronizeContextScoped OwnContextScoped => Context.DataLoaderContextScoped;

            public IPluginsSynchronizeContextScoped DataLoaderContextScoped => Context.DataLoaderContextScoped;

            public Node CurrentNode => Context.CurrentNode;

            public IPluginAccessor<INodePlugin> CustomService => Context.CustomService;

            public LoadedNodeSet LoadedNodeSet => Context.LoadedNodeSet;

            public Dictionary<string, object> ParentResult => Context.ParentResult;

            public IPluginServiceManager PluginServiceManager => Context.PluginServiceManager;

            public IProcessingContext ProcessingContext => Context.ProcessingContext;

            public string Path => Context.Path;

            public string NodeKey => Context.NodeKey;

            public string Action => Context.Action;

            public string Operation => Context.Operation;

            public ParameterResolverValue Evaluate(string expressionName, IList<ParameterResolverValue> oldargs = null)
            {
                return Context.Evaluate(expressionName, oldargs);
            }
        }

        public class LoaderPluginReadContext: LoaderPluginContext, IDataLoaderReadContext {
            public LoaderPluginReadContext(NodeExecutionContext nodectx) : base(nodectx) { }

            public List<Dictionary<string, object>> Results => Context.Results;
        }
        public class LoaderPluginWriteContext : LoaderPluginContext, IDataLoaderWriteContext {
            public LoaderPluginWriteContext(NodeExecutionContext nodectx) : base(nodectx) { }
            public Dictionary<string, object> Row => Context.Row;

        }

        // TODO - may be use the loader read context?
        /// <summary>
        /// A context passed to the newer Prepare action
        /// </summary>
        //public class LoaderPluginPrepareContext : NodeExecutionContextProxy, IDataLoaderPrepareContext { 
        //    public LoaderPluginPrepareContext(NodeExecutionContext nodectx) : base(nodectx) { }

        //    public IPluginsSynchronizeContextScoped OwnContextScoped => Context.DataLoaderContextScoped;

        //    public IPluginsSynchronizeContextScoped DataLoaderContextScoped => Context.DataLoaderContextScoped;

        //    public Node CurrentNode => Context.CurrentNode;

        //    public IPluginAccessor<INodePlugin> CustomService => Context.CustomService;

        //    public LoadedNodeSet LoadedNodeSet => Context.LoadedNodeSet;

        //    public Dictionary<string, object> ParentResult => Context.ParentResult;

        //    public IPluginServiceManager PluginServiceManager => Context.PluginServiceManager;

        //    public IProcessingContext ProcessingContext => Context.ProcessingContext;

        //    public string Path => Context.Path;

        //    public string NodeKey => Context.NodeKey;

        //    public string Action => Context.Action;

        //    public string Operation => Context.Operation;

        //    public List<Dictionary<string, object>> Results => Context.Results;

        //    public ParameterResolverValue Evaluate(string expressionName, IList<ParameterResolverValue> oldargs = null) {
        //        return Context.Evaluate(expressionName, oldargs);
        //    }
        //}


        #endregion



    }
}
