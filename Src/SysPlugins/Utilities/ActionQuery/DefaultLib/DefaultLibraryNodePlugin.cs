using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Settings;
using System.IO;
using System.Collections;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using Ccf.Ck.Processing.Web.ResponseBuilder;
using Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes;
using static Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes.BaseAttribute;

namespace Ccf.Ck.SysPlugins.Utilities
{
    [Library("default", LibraryContextFlags.Node)]
    public class DefaultLibraryNodePlugin<HostInterface> : DefaultLibraryBase<HostInterface> where HostInterface : class
    {
        public DefaultLibraryNodePlugin()
        {

        }
        private static readonly DefaultLibraryNodePlugin<HostInterface> _Instance = new DefaultLibraryNodePlugin<HostInterface>();
        public static DefaultLibraryNodePlugin<HostInterface> Instance
        {
            get
            {
                return _Instance;
            }
        }

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
                case nameof(CSettingLoader):
                    return CSettingLoader;
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

        [Function(nameof(BailOut), "")]
        public ParameterResolverValue BailOut(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is INodePluginContext rctx)
            {
                rctx.BailOut();
            }
            return new ParameterResolverValue(null);
        }

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

        [Function(nameof(DictFromParameters), "")]
        [Parameter(0, "parameternames", "comma separated list of parameters to fetch", TypeFlags.String)]
        public ParameterResolverValue DictFromParameters(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (args.Length == 1)
            {
                var dict = new Dictionary<string, ParameterResolverValue>(); // The new dictionary
                var plist = args[0].Value as string;
                var dlctx = ctx as INodePluginContext;
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

        #region

        [Function(nameof(AddResult), "")]
        public ParameterResolverValue AddResult(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is INodePluginReadContext rctx)
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
                    // TODO: May be we should return the result as dictionary so that we can manipulate it (depends on where the library is going)
                    return new ParameterResolverValue(null);
                }
            }
            else
            {
                throw new InvalidOperationException("AddResult can be used only in Read actions");
            }
        }

        [Function(nameof(HasResults), "")]
        public ParameterResolverValue HasResults(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is INodePluginReadContext rctx)
            {
                return new ParameterResolverValue(rctx.Results.Count > 0);
            }
            else
            {
                return new ParameterResolverValue(false);
            }
        }

        [Function(nameof(ModifyResult), "")]
        public ParameterResolverValue ModifyResult(HostInterface ctx, ParameterResolverValue[] args)
        {
            Dictionary<string, object> result = null;
            if (ctx is INodePluginReadContext rctx)
            {
                if (rctx.Results.Count > 0)
                {
                    if (args.Length > 1)
                    {
                        var index = Convert.ToUInt32(args[0].Value);
                        if (index >= 0 && index < rctx.Results.Count)
                        {
                            result = rctx.Results[rctx.Results.Count - 1];
                        }
                        else
                        {
                            throw new IndexOutOfRangeException("ModifyResult specified indes (first arg) is out of range");
                        }
                    }
                    else
                    {
                        throw new ArgumentException("ModifyResult requires some arguments.");
                    }
                }
                else
                {
                    throw new InvalidOperationException("There are no results created yet. Use AddResult or register the plugin in another phase (after the node execution).");
                }
            }
            else if (ctx is INodePluginWriteContext wctx)
            {
                throw new InvalidOperationException("ModifyResult is not available for write actions - use SetResult instead.");
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }

            if (args.Length == 2 && args[1].Value is IDictionary _dict)
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
                var count = (args.Length - 1) / 2;
                object lastvalue = null;
                for (int i = 0; i < count; i++)
                {
                    var name = args[1 + i * 2].Value as string;
                    var value = args[i * 2 + 2].Value;
                    if (name != null)
                    {
                        result[name] = value;
                        lastvalue = value;
                    }
                    else
                    {
                        throw new ArgumentException($"ModifyResult works with 1 argument or with even number of arguments after the index (first arg) repeatig name, value pattern. The {i * 2} argument is not a string.");
                    }
                }
                return new ParameterResolverValue(lastvalue);
            }
        }

        [Function(nameof(SetResult), "")]
        public ParameterResolverValue SetResult(HostInterface ctx, ParameterResolverValue[] args)
        {
            Dictionary<string, object> result = null;
            if (ctx is INodePluginReadContext rctx)
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
            else if (ctx is INodePluginWriteContext wctx)
            {
                result = wctx.Row;
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }

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
                object lastvalue = null;
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

        [Function(nameof(ClearResultExcept), "")]
        public ParameterResolverValue ClearResultExcept(HostInterface ctx, ParameterResolverValue[] args)
        {
            Dictionary<string, object> result = null;
            if (ctx is INodePluginReadContext rctx)
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
            else if (ctx is INodePluginWriteContext wctx)
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
        private Dictionary<string, object> _GetResult(HostInterface ctx)
        {
            Dictionary<string, object> result = null;
            if (ctx is INodePluginReadContext rctx)
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
            else if (ctx is INodePluginWriteContext wctx)
            {
                result = wctx.Row;
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }
            return result;
        }

        [Function(nameof(GetStatePropertyName), "")]
        public ParameterResolverValue GetStatePropertyName(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is INodePluginContext dtx)
            {
                return new ParameterResolverValue(dtx.DataState.StatePropertyName);
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(ResetResultState), "")]
        public ParameterResolverValue ResetResultState(HostInterface ctx, ParameterResolverValue[] args)
        {
            var result = _GetResult(ctx);
            if (ctx is INodePluginContext dtx)
            {
                dtx.DataState.SetUnchanged(result);
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(SetResultDeleted), "")]
        public ParameterResolverValue SetResultDeleted(HostInterface ctx, ParameterResolverValue[] args)
        {
            var result = _GetResult(ctx);
            if (ctx is INodePluginContext dtx)
            {
                dtx.DataState.SetDeleted(result);
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(SetResultNew), "")]
        public ParameterResolverValue SetResultNew(HostInterface ctx, ParameterResolverValue[] args)
        {
            var result = _GetResult(ctx);
            if (ctx is INodePluginContext dtx)
            {
                dtx.DataState.SetNew(result);
            }
            return new ParameterResolverValue(null);
        }

        [Function(nameof(SetResultUpdated), "")]
        public ParameterResolverValue SetResultUpdated(HostInterface ctx, ParameterResolverValue[] args)
        {
            var result = _GetResult(ctx);
            if (ctx is INodePluginContext dtx)
            {
                dtx.DataState.SetUpdated(result);
            }
            return new ParameterResolverValue(null);
        }

        /// <summary>
        /// ResultsCount(): int
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Function(nameof(ResultsCount), "")]
        public ParameterResolverValue ResultsCount(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is INodePluginReadContext rctx)
            {
                return new ParameterResolverValue(rctx.Results.Count);
            }
            else if (ctx is INodePluginWriteContext wctx)
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
        [Function(nameof(GetResult), "")]
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

        [Function(nameof(RemoveResult), "")]
        public ParameterResolverValue RemoveResult(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is INodePluginReadContext rctx)
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
            else if (ctx is INodePluginWriteContext wctx)
            {
                throw new Exception("RemoveResult cannot be used in write actions.");
            }
            else
            {
                throw new Exception("The impossible happend! The node context is nor read, nor write context");
            }
        }

        [Function(nameof(RemoveAllResults), "Removes all results collected in read operation so far.")]
        public ParameterResolverValue RemoveAllResults(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is INodePluginReadContext rctx)
            {
                rctx.Results.Clear();
                return new ParameterResolverValue(null);
            }
            else if (ctx is INodePluginWriteContext wctx)
            {
                throw new Exception("RemoveAllResults cannot be used in write actions.");
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
        [Function(nameof(GetAllResults), "")]
        public ParameterResolverValue GetAllResults(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is INodePluginReadContext rctx)
            {
                return DefaultLibraryBase<HostInterface>.ConvertFromGenericData(rctx.Results);
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

        #endregion

        #region Information about what is happening
        /// <summary>
        /// Returns the module phisical path (without args) with single argument combines it with the module path.
        /// </summary>
        /// <param name="_ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        [Function(nameof(ModulePath), "")]
        public ParameterResolverValue ModulePath(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
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
                            throw new ArgumentException("The argument of ModulePath, if supplied, must contain path without .. and not starting with a slash");
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

        [Function(nameof(ModuleName), "")]
        public ParameterResolverValue ModuleName(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
            return new ParameterResolverValue(ctx.ProcessingContext.InputModel.Module);
        }

        [Function(nameof(NodePath), "")]
        public ParameterResolverValue NodePath(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
            return new ParameterResolverValue(ctx.ProcessingContext.InputModel.Nodepath);

        }

        [Function(nameof(NodeKey), "")]
        public ParameterResolverValue NodeKey(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
            return new ParameterResolverValue(ctx.NodeKey);
        }

        [Function(nameof(Action), "")]
        public ParameterResolverValue Action(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
            return new ParameterResolverValue(ctx.Action);
        }

        [Function(nameof(Operation), "")]
        public ParameterResolverValue Operation(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
            return new ParameterResolverValue(ctx.Operation);
        }
        #endregion

        #region Settings

        [Function(nameof(CSetting), "")]
        public ParameterResolverValue CSetting(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
            if (args.Length < 1 || args.Length > 2)
            {
                throw new ArgumentException($"CSetting accepts 1 or two arguments, but {args.Length} were given.");
            }
            var name = args[0].Value as string;
            if (name == null)
            {
                throw new ArgumentException($"CSetting first argument must be string - the name of the custom setting to obtain.");
            }
            if (ctx.OwnContextScoped.CustomSettings.TryGetValue(name, out string val))
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

        [Function(nameof(CSettingLoader), "")]
        public ParameterResolverValue CSettingLoader(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
            if (args.Length < 1 || args.Length > 2)
            {
                throw new ArgumentException($"CSettingLoader accepts 1 or two arguments, but {args.Length} were given.");
            }
            var name = args[0].Value as string;
            if (name == null)
            {
                throw new ArgumentException($"CSettingLoader first argument must be string - the name of the custom setting to obtain.");
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
