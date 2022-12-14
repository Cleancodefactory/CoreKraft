using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Settings;
using System.IO;
using System.Collections;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes;
using static Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes.BaseAttribute;

namespace Ccf.Ck.SysPlugins.Utilities
{
    [Library("default", LibraryContextFlags.Main)]
    public class DefaultLibraryLoaderPlugin<HostInterface> : DefaultLibraryBase<HostInterface> where HostInterface : class
    {
        public DefaultLibraryLoaderPlugin()
        {

        }
        private static readonly DefaultLibraryLoaderPlugin<HostInterface> _Instance = new DefaultLibraryLoaderPlugin<HostInterface>();
        public static DefaultLibraryLoaderPlugin<HostInterface> Instance { get { return _Instance; } }


        public override HostedProc<HostInterface> GetProc(string name)
        {
            // TODO: Return local methods
            switch (name)
            {
                case nameof(NodePath):
                    return NodePath;
                case nameof(NodeKey):
                    return NodeKey;
                case nameof(Action):
                    return Action;
                case nameof(Operation):
                    return Operation;

                case nameof(BailOut):
                    return BailOut;
                case nameof(OverrideResponseData):
                    return OverrideResponseData;
                case nameof(ForceJSONResponse):
                    return ForceJSONResponse;
                case nameof(ForceTextResponse):
                    return ForceTextResponse;
                case nameof(DictFromParameters):
                    return DictFromParameters;

                case nameof(AddResult):
                    return AddResult;
                case nameof(HasResults):
                    return HasResults;
                case nameof(SetResult):
                    return SetResult;
                case nameof(ClearResultExcept):
                    return ClearResultExcept;
                case nameof(ResultsCount):
                    return ResultsCount;
                case nameof(GetResult):
                    return GetResult;
                case nameof(GetAllResults):
                    return GetAllResults;
                case nameof(RemoveResult):
                    return RemoveResult;
                case nameof(RemoveAllResults):
                    return RemoveAllResults;
                case nameof(ModifyResult):
                    return ModifyResult;


                case nameof(CSetting):
                    return CSetting;
                case nameof(ModuleName):
                    return ModuleName;
                case nameof(ModulePath):
                    return ModulePath;

                case nameof(ResetResultState):
                    return ResetResultState;
                case nameof(SetResultDeleted):
                    return SetResultDeleted;
                case nameof(SetResultNew):
                    return SetResultNew;
                case nameof(SetResultUpdated):
                    return SetResultUpdated;
                case nameof(GetStatePropertyName):
                    return GetStatePropertyName;
            }
            return base.GetProc(name);
        }

        //No doc ATM
        [Function(nameof(BailOut), "")]
        public ParameterResolverValue BailOut(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is IDataLoaderContext rctx)
            {
                rctx.BailOut();
            }
            return new ParameterResolverValue(null);
        }

        //No doc ATM
        [Function(nameof(OverrideResponseData), "")]
        public ParameterResolverValue OverrideResponseData(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length != 1) throw new ArgumentException("OverrideResponseData requires exactly 1 argument");
            if (ctx is IDataLoaderContext ctx1)
            {
                ctx1.ProcessingContext.ReturnModel.Data = args[0].Value;
            }
            else if (ctx is INodePluginContext ctx2)
            {
                ctx2.ProcessingContext.ReturnModel.Data = args[0].Value;
            }
            return args[0];
        }

        //No doc ATM
        [Function(nameof(ForceJSONResponse), "")]
        public ParameterResolverValue ForceJSONResponse(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is IDataLoaderContext ctx1)
            {
                ctx1.ProcessingContext.ReturnModel.ResponseBuilder = new JsonResponseBuilder(new ProcessingContextCollection(new List<IProcessingContext> { ctx1.ProcessingContext }));
            }
            else if (ctx is INodePluginContext ctx2)
            {
                ctx2.ProcessingContext.ReturnModel.ResponseBuilder = new JsonResponseBuilder(new ProcessingContextCollection(new List<IProcessingContext> { ctx2.ProcessingContext }));
            }
            return new ParameterResolverValue(null);
        }

        //No doc ATM
        [Function(nameof(ForceTextResponse), "")]
        public ParameterResolverValue ForceTextResponse(HostInterface ctx, ParameterResolverValue[] args)
        {
            string contentType = null;
            if (args.Length > 0)
            {
                contentType = Convert.ToString(args[0].Value);
            }
            if (ctx is IDataLoaderContext ctx1)
            {
                ctx1.ProcessingContext.ReturnModel.ResponseBuilder = new TextResponseBuilder(new ProcessingContextCollection(new List<IProcessingContext> { ctx1.ProcessingContext }), contentType);
            }
            else if (ctx is INodePluginContext ctx2)
            {
                ctx2.ProcessingContext.ReturnModel.ResponseBuilder = new TextResponseBuilder(new ProcessingContextCollection(new List<IProcessingContext> { ctx2.ProcessingContext }), contentType);
            }
            return new ParameterResolverValue(contentType);
        }

        //No doc ATM
        [Function(nameof(DictFromParameters), "")]
        [Parameter(0, "parameternames", "comma separated list of parameters to fetch", TypeFlags.String)]
        public ParameterResolverValue DictFromParameters(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length == 1)
            {
                var dict = new Dictionary<string, ParameterResolverValue>(); // The new dictionary
                var plist = args[0].Value as string;
                var dlctx = ctx as IDataLoaderContext;
                if (dlctx == null) throw new Exception("The context is invalid. IDataLoaderContext expected.");

                if (string.IsNullOrWhiteSpace(plist))
                {
                    return new ParameterResolverValue(dict);
                }
                string[] slist = plist.Split(',');
                if (slist == null || slist.Length == 0)
                {
                    return new ParameterResolverValue(dict);
                }
                foreach (string _paramName in slist)
                {
                    var paramName = _paramName.Trim();
                    dict[paramName] = dlctx.Evaluate(paramName);
                }
                return new ParameterResolverValue(dict);
            }
            else
            {
                throw new ArgumentException("DictFromParameters accpets only one argument.");
            }

        }

        #region results

        [Function(nameof(AddResult), "Works only in read actions.Creates a new result(resulting row).After it until AddResult is called again SetResult works on the recently added result.Can be called without arguments to create an empty result.")]
        [Parameter(0, "Name", "argument name (repeating)", TypeFlags.Optional | TypeFlags.Varying)]
        [Parameter(1, "Value", "argument value (repeating)", TypeFlags.Optional | TypeFlags.Varying)]
        [Result("Creates a new resulting row", TypeFlags.Varying)]
        public ParameterResolverValue AddResult(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is IDataLoaderReadContext rctx)
            {
                var result = new Dictionary<string, object>();
                if (args.Length == 1 && args[0].Value is IDictionary _dict)
                {
                    foreach (DictionaryEntry e in _dict)
                    {
                        result.TryAdd(Convert.ToString(e.Key), ParameterResolverValue.Strip(e.Value));
                    }
                    rctx.Results.Add(result);
                    return new ParameterResolverValue(null);
                }
                else if (args.Length == 1 && args[0].Value is IEnumerable _enumerable)
                {
                    foreach (var e in _enumerable)
                    {
                        if (ParameterResolverValue.Strip(e) is IDictionary dict)
                        {
                            result = new Dictionary<string, object>();
                            foreach (DictionaryEntry dictEntry in dict)
                            {
                                result.TryAdd(Convert.ToString(dictEntry.Key), ParameterResolverValue.Strip(dictEntry.Value));
                            }
                            rctx.Results.Add(result);
                        }
                    }
                    return new ParameterResolverValue(null);
                }
                else
                {
                    var count = args.Length / 2;
                    for (int i = 0; i < count; i++)
                    {
                        var name = args[i * 2].Value as string;
                        var value = args[i * 2 + 1].Value;
                        if (name != null)
                        {
                            result[name] = value;
                        }
                        else
                        {
                            throw new ArgumentException($"AddResult works with no arguments or with even number of arguments repeatig name, value pattern. The {i * 2} argument is not a string.");
                        }
                    }
                    rctx.Results.Add(result);
                }
                // TODO: May be we should return the result as dictionary so that we can manipulate it (depends on where the library is going)
                return new ParameterResolverValue(null);
            }
            else
            {
                throw new InvalidOperationException("AddResult can be used only in Read actions");
            }
        }

        /// <summary>
        /// NEW NEEDS TO BE ADDED
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Function(nameof(RemoveAllResults), "Removes all results collected in read operation so far.")]
        public ParameterResolverValue RemoveAllResults(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is IDataLoaderReadContext rctx)
            {
                rctx.Results.Clear();
                return new ParameterResolverValue(null);
            }
            else if (ctx is IDataLoaderWriteContext wctx)
            {
                throw new Exception("RemoveAllResults cannot be used in write actions.");
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }
        }

        [Function(nameof(HasResults), "Returns true or false depending on if any result exists. In write actions always returns true.")]
        [Result("Returns a boolean", TypeFlags.Bool)]
        public ParameterResolverValue HasResults(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is IDataLoaderReadContext rctx)
            {
                return new ParameterResolverValue(rctx.Results.Count > 0);
            }
            else
            {
                return new ParameterResolverValue(false);
            }
        }
        private Dictionary<string, object> _GetResult(HostInterface ctx)
        {
            Dictionary<string, object> result = null;
            if (ctx is IDataLoaderReadContext rctx)
            {
                if (rctx.Results.Count > 0)
                {
                    result = rctx.Results[rctx.Results.Count - 1];
                }
                else
                {
                    throw new InvalidOperationException("There are no results created yet. Use AddResult or register the plugin in another phase (after the node execution).");
                }
            }
            else if (ctx is IDataLoaderWriteContext wctx)
            {
                result = wctx.Row;
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }
            return result;
        }

        [Function(nameof(GetStatePropertyName), "Returns the property name of the data state property. Use this if you want to write script able to work with CoreKraft complied with different name for the state property.")]
        [Result("Returns a string or null", TypeFlags.String | TypeFlags.Null)]
        public ParameterResolverValue GetStatePropertyName(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is IDataLoaderContext dtx)
            {
                return new ParameterResolverValue(dtx.DataState.StatePropertyName);
            }
            return new ParameterResolverValue(null);
        }

        private ParameterResolverValue ProcessResult(HostInterface ctx, Action<IDataStateHelperProvider<IDictionary<string, object>>, Dictionary<string, object>> action, ParameterResolverValue[] args) {
            int? index = null;
            if (args.Length > 1) {
                index = Convert.ToInt32(args[0].Value);
            }
            IDataStateHelperProvider<IDictionary<string, object>> stateHelper = ctx as IDataStateHelperProvider<IDictionary<string, object>>;
            if (ctx is INodePluginContextWithResults ctx_r) {
                if (ctx_r.Results != null) {
                    if (index != null) {
                        if (index < 0) {
                            ctx_r.Results.ForEach(r => action(stateHelper, r));
                        } else if (index >= 0 && index < ctx_r.Results.Count) {
                            action(stateHelper, ctx_r.Results[index.Value]);
                        } else {
                            throw new IndexOutOfRangeException("The index argument must be an index of a result or a negative integer");
                        }
                    } else {
                        if (ctx_r.Results.Count > 0) {
                            action(stateHelper, ctx_r.Results.Last());
                        }
                    }
                }
            } else if (ctx is IDataLoaderWriteContext dtx) {
                if (dtx.Row != null) action(stateHelper, dtx.Row);
            }
            return new ParameterResolverValue(null);
        }


        [Function(nameof(ResetResultState), "Resets the state of the current result to unchanged.")]
        public ParameterResolverValue ResetResultState(HostInterface ctx, ParameterResolverValue[] args)
        {
            return ProcessResult(ctx, (dh, r) => dh.DataState.SetUnchanged(r), args);
            //var result = _GetResult(ctx);
            //if (ctx is IDataLoaderContext dtx)
            //{
            //    dtx.DataState.SetUnchanged(result);
            //}
            //return new ParameterResolverValue(null);
        }

        [Function(nameof(SetResultDeleted), "Sets the state of the current result to deleted. If you want to impact the current execution process this should be set in a node script executed in beforenodeaction phase.")]
        public ParameterResolverValue SetResultDeleted(HostInterface ctx, ParameterResolverValue[] args)
        {
            return ProcessResult(ctx, (dh, r) => dh.DataState.SetDeleted(r), args);
            //var result = _GetResult(ctx);
            //if (ctx is IDataLoaderContext dtx)
            //{
            //    dtx.DataState.SetDeleted(result);
            //}
            //return new ParameterResolverValue(null);
        }

        [Function(nameof(SetResultNew), "Sets the state of the result (top on read, current on write) to new.")]
        public ParameterResolverValue SetResultNew(HostInterface ctx, ParameterResolverValue[] args)
        {
            return ProcessResult(ctx, (dh, r) => dh.DataState.SetNew(r), args);
            //var result = _GetResult(ctx);
            //if (ctx is IDataLoaderContext dtx)
            //{
            //    dtx.DataState.SetNew(result);
            //}
            //return new ParameterResolverValue(null);
        }

        [Function(nameof(SetResultUpdated), "Sets the state of the result (top on read, current on write) to changed.")]
        public ParameterResolverValue SetResultUpdated(HostInterface ctx, ParameterResolverValue[] args)
        {
            return ProcessResult(ctx, (dh, r) => dh.DataState.SetUpdated(r), args);
            //var result = _GetResult(ctx);
            //if (ctx is IDataLoaderContext dtx)
            //{
            //    dtx.DataState.SetUpdated(result);
            //}
            //return new ParameterResolverValue(null);
        }

        // Check Length
        [Function(nameof(ModifyResult), "Works only in read actions. Modifies the result indicated by index, by setting the specified values one by one or by using dictionary in the same fashion like SetResult.")]
        public ParameterResolverValue ModifyResult(HostInterface ctx, ParameterResolverValue[] args)
        {
            throw new InvalidOperationException("ModifyResult is supported only in node plugins. DataLoaders must produce results sequentially in order to stick to universaly expected behaviour!");
        }

        [Function(nameof(SetResult), "Sets values of the top or current result")]
        [Parameter(0, "Dictionary", "Dictionary to supply keys and values to set",TypeFlags.Dict | TypeFlags.Optional)]
        [ParameterPattern(1, "Key_Val_Params", "Pairs of key names / values to set",TypeFlags.String,TypeFlags.Varying)]
        [Result("Returns the last set value - not recommended to use",TypeFlags.Varying)]
        public ParameterResolverValue SetResult(HostInterface ctx, ParameterResolverValue[] args)
        {
            Dictionary<string, object> result = null;
            if (ctx is IDataLoaderReadContext rctx)
            {
                if (rctx.Results.Count > 0)
                {
                    result = rctx.Results[rctx.Results.Count - 1];
                }
                else
                {
                    throw new InvalidOperationException("There are no results created yet. Use AddResult or register the plugin in another phase (after the node execution).");
                }
            }
            else if (ctx is IDataLoaderWriteContext wctx)
            {
                result = wctx.Row;
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }
            object lastvalue = null;
            if (args.Length == 1 && args[0].Value is IDictionary _dict)
            {
                foreach (DictionaryEntry e in _dict)
                {
                    result[Convert.ToString(e.Key)] = ParameterResolverValue.Strip(e.Value);
                }
                // In this case we cannot guarantee which one is the last value and we return null in order to avoid misunderstandings.
                return new ParameterResolverValue(null);
            }
            else
            {
                var count = args.Length / 2;

                for (int i = 0; i < count; i++)
                {
                    var name = args[i * 2].Value as string;
                    var value = args[i * 2 + 1].Value;
                    if (name != null)
                    {
                        result[name] = value;
                        lastvalue = value;
                    }
                    else
                    {
                        throw new ArgumentException($"SetResult works with no arguments or with even number of arguments repeatig name, value pattern. The {i * 2} argument is not a string.");
                    }
                }
                return new ParameterResolverValue(lastvalue);
            }
        }

        [Function(nameof(ClearResultExcept), "Clears the result (the top result on read, the current on write), but leaves the values of the listed keys intact. Called without arguments clears the result completely.")]
        [Parameter(0, "Keys", "Keys to keep intact", TypeFlags.Varying)]
        [Result("Returns the clearned result", TypeFlags.Varying)]
        public ParameterResolverValue ClearResultExcept(HostInterface ctx, ParameterResolverValue[] args)
        {
            Dictionary<string, object> result = null;
            if (ctx is IDataLoaderReadContext rctx)
            {
                if (rctx.Results.Count > 0)
                {
                    result = rctx.Results[rctx.Results.Count - 1];
                }
                else
                {
                    throw new InvalidOperationException("There are no results created yet. Use AddResult or register the plugin in another phase (after the node execution).");
                }
            }
            else if (ctx is IDataLoaderWriteContext wctx)
            {
                result = wctx.Row;
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }
            List<string> preservekeys = new List<string>(10);
            if (args.Length == 1 && args[0].Value is IList list)
            {
                foreach (object e in list)
                {
                    preservekeys.Add(Convert.ToString(ParameterResolverValue.Strip(e)));
                }
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var name = Convert.ToString(args[i].Value);

                    if (!string.IsNullOrEmpty(name))
                    {
                        preservekeys.Add(name);
                    }
                }
            }
            var keys = result.Keys.Where(k => !preservekeys.Contains(k));
            foreach (var k in keys)
            {
                result.Remove(k);
            }
            return new ParameterResolverValue(preservekeys.ConvertAll(s => new ParameterResolverValue(s)));
        }

        /// <summary>
        /// ResultsCount(): int
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Function(nameof(ResultsCount), "Returns the number of result dictionaries in read actions and always 1 in write actions.")]
        [Result("Returns a int32", TypeFlags.Int)]
        public ParameterResolverValue ResultsCount(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is IDataLoaderReadContext rctx)
            {
                return new ParameterResolverValue(rctx.Results.Count);
            }
            else if (ctx is IDataLoaderWriteContext wctx)
            {
                return new ParameterResolverValue(1);
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }
        }
        /// <summary>
        /// Read: GetResult(index) : Dict
        /// Write: GetResult() : Dict
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// Check Length
        [Function(nameof(GetResult), "In read actions gets result specified by index. In write actions always returns the only result (any arguments are ignored). The return value is a Dict with copy of the result and not the result itself.")]
        [Parameter(0, "Index", "index of result", TypeFlags.Int)]
        [Result("Returns a dictionary COPY of result", TypeFlags.Dict)]
        public ParameterResolverValue GetResult(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is INodePluginReadContext rctx)
            {
                int index = -1;
                if (args.Length < 1)
                {//throw new ArgumentException("GetResult in read actions requires an argument - the index of the result to return.");
                    index = rctx.Results.Count - 1;
                }
                else
                {
                    index = Convert.ToInt32(args[0].Value);
                }
                if (index >= 0 && index < rctx.Results.Count)
                {
                    return DefaultLibraryBase<HostInterface>.ConvertFromGenericData(rctx.Results[index]);
                }
                return new ParameterResolverValue(null);
            }
            else if (ctx is INodePluginWriteContext wctx)
            {
                return DefaultLibraryBase<HostInterface>.ConvertFromGenericData(wctx.Row);
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }
        }

        [Function(nameof(RemoveResult), "Removes result specified by index. index must be between >=0 and < ResultsCount(). In write actions throws an exception.")]
        [Parameter(0, "Index", "index to remove result from", TypeFlags.Int)]
        [Result("Returns the removed result or throws and exception", TypeFlags.Varying)]
        public ParameterResolverValue RemoveResult(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is IDataLoaderReadContext rctx)
            {
                if (args.Length < 1) throw new ArgumentException("RemoveResult in read actions requires an argument - the index of the result to return.");
                var index = Convert.ToInt32(args[0].Value);
                if (index >= 0 && index < rctx.Results.Count)
                {
                    var result = DefaultLibraryBase<HostInterface>.ConvertFromGenericData(rctx.Results[index]);
                    rctx.Results.RemoveAt(index);
                    return result;
                }
                return new ParameterResolverValue(null);
            }
            else if (ctx is IDataLoaderWriteContext wctx)
            {
                throw new Exception("RemoveResult cannot be used in write actions.");
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }
        }
        /// <summary>
        /// Read: GetAllResults():List
        /// Write: GetAllResults():Dict
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Function(nameof(GetAllResults), "In read actions returns List of Dict objects (see List and Dict above) which are copies of all the results accumulated so far in the current node. In write actions returns a Dict with the current row.")]
        public ParameterResolverValue GetAllResults(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is IDataLoaderReadContext rctx)
            {
                return DefaultLibraryBase<HostInterface>.ConvertFromGenericData(rctx.Results);
            }
            else if (ctx is IDataLoaderWriteContext wctx)
            {
                return DefaultLibraryBase<HostInterface>.ConvertFromGenericData(wctx.Row);
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }
        }
        #endregion

        #region Information about what is happening
        /// <summary>
        /// Returns the module phisical path (without args) with single argument combines it with the module path.
        /// </summary>
        /// <param name="_ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Function(nameof(ModulePath), "Returns the physical module path of the current module. If argument is passed combines them. For example to get the physical path of the module's Data directory use ModulePath('Data').")]
        [Parameter(0, "Subpath", "subpath", TypeFlags.String)]
        [Result("Returns the path as string", TypeFlags.String)]
        public ParameterResolverValue ModulePath(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            var kgcf = ctx.PluginServiceManager.GetService<KraftGlobalConfigurationSettings>(typeof(KraftGlobalConfigurationSettings));
            if (ctx != null && kgcf != null)
            {
                var path = kgcf.GeneralSettings.ModulesRootFolder(ctx.ProcessingContext.InputModel.Module);
                if (!path.EndsWith("\\") && !path.EndsWith("/"))
                {
                    path = path + "/";
                }
                path = path + ctx.ProcessingContext.InputModel.Module + "/";
                if (args.Length > 0)
                {
                    var subpath = args[0].Value as string;
                    if (string.IsNullOrWhiteSpace(subpath))
                    {
                        throw new ArgumentException("The argument of ModulePath, if supplied, has to be a string - the path to combine with module path");
                    }
                    else
                    {
                        if (subpath.IndexOf("..") >= 0 || subpath.StartsWith("/") || subpath.StartsWith("\\"))
                        {
                            throw new ArgumentException($"The argument of ModulePath ({subpath}), if supplied, must contain path without .. and not starting with a slash");
                        }
                        return new ParameterResolverValue(Path.Combine(path, subpath));
                    }
                }
                else
                {
                    return new ParameterResolverValue(path);
                }
            }
            else
            {
                throw new Exception("Cannot obtain the context or the global settings.");
            }
        }

        [Function(nameof(ModuleName), "Returns module name")]
        public ParameterResolverValue ModuleName(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            return new ParameterResolverValue(ctx.ProcessingContext.InputModel.Module);
        }

        [Function(nameof(NodePath), "Returns the full path of the node from the current nodeset. The result is a string containg the node names in the path separated with '.' ")]
        public ParameterResolverValue NodePath(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            return new ParameterResolverValue(ctx.ProcessingContext.InputModel.Nodepath);

        }

        [Function(nameof(NodeKey), "Returns the key name of the current node.")]
        public ParameterResolverValue NodeKey(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            return new ParameterResolverValue(ctx.NodeKey);
        }

        [Function(nameof(Action), "Returns the action: read or write as string.")]
        public ParameterResolverValue Action(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            return new ParameterResolverValue(ctx.Action);
        }

        [Function(nameof(Operation), "Returns the operation under which the script is executed. Applies to both data loader and node scripts. The returned value is string and can be one of these select, insert, update, delete.")]
        public ParameterResolverValue Operation(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            return new ParameterResolverValue(ctx.Operation);
        }
        #endregion

        #region Settings
        [Function(nameof(CSetting), "Gets a custom setting by name. Custom settings are specified in the plugin configurations in Configuration.json or in override sections in appsettings")]
        [Parameter(0, "Name", "setting name to find", TypeFlags.Varying)]
        [Result("Returns custom setting or null", TypeFlags.String | TypeFlags.Null)]
        public ParameterResolverValue CSetting(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            if (args.Length < 1 || args.Length > 2)
            {
                throw new ArgumentException($"CSetting accepts 1 or two arguments, but {args.Length} were given.");
            }
            var name = args[0].Value as string;
            if (name == null)
            {
                throw new ArgumentException($"CSetting first argument must be string - the name of the custom setting to obtain.");
            }
            if (ctx.DataLoaderContextScoped.CustomSettings.TryGetValue(name, out string val))
            {
                return new ParameterResolverValue(val);
            }
            if (args.Length > 1)
            {
                return args[1];
            }
            else
            {
                return new ParameterResolverValue(null);
            }
        }

        #endregion
    }
}
