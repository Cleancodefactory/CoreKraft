using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Settings;
using System.IO;
using System.Collections;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class DefaultLibraryLoaderPlugin<HostInterface>: DefaultLibraryBase<HostInterface> where HostInterface: class
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

                case nameof(AddResult):
                    return AddResult;
                case nameof(HasResults):
                    return HasResults;
                case nameof(SetResult):
                    return SetResult;

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

        #region results

        public ParameterResolverValue AddResult(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is IDataLoaderReadContext rctx)
            {
                var result = new Dictionary<string, object>();
                if (args.Length == 1 && args[0].Value is IDictionary _dict) {
                    foreach (DictionaryEntry e in _dict) {
                        result.TryAdd(Convert.ToString(e.Key), ParameterResolverValue.Strip(e.Value));
                    }
                    rctx.Results.Add(result);
                    return new ParameterResolverValue(null);
                } else {
                    
                    var count = args.Length / 2;
                    for (int i = 0; i < count; i++) {
                        var name = args[i * 2].Value as string;
                        var value = args[i * 2 + 1].Value;
                        if (name != null) {
                            result[name] = value;
                        } else {
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
        
        public ParameterResolverValue GetStatePropertyName(HostInterface ctx, ParameterResolverValue[] args) {
            if (ctx is IDataLoaderContext dtx) {
                return new ParameterResolverValue(dtx.DataState.StatePropertyName);
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue ResetResultState(HostInterface ctx, ParameterResolverValue[] args)
        {
            var result = _GetResult(ctx);
            if (ctx is IDataLoaderContext dtx)
            {
                dtx.DataState.SetUnchanged(result);
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue SetResultDeleted(HostInterface ctx, ParameterResolverValue[] args)
        {
            var result = _GetResult(ctx);
            if (ctx is IDataLoaderContext dtx)
            {
                dtx.DataState.SetDeleted(result);
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue SetResultNew(HostInterface ctx, ParameterResolverValue[] args)
        {
            var result = _GetResult(ctx);
            if (ctx is IDataLoaderContext dtx)
            {
                dtx.DataState.SetNew(result);
            }
            return new ParameterResolverValue(null);
        }
        public ParameterResolverValue SetResultUpdated(HostInterface ctx, ParameterResolverValue[] args)
        {
            var result = _GetResult(ctx);
            if (ctx is IDataLoaderContext dtx)
            {
                dtx.DataState.SetUpdated(result);
            }
            return new ParameterResolverValue(null);
        }
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
            if (args.Length == 1 && args[0].Value is IDictionary _dict) {
                foreach (DictionaryEntry e in _dict) {
                    result[Convert.ToString(e.Key)] = ParameterResolverValue.Strip(e.Value);
                }
                // In this case we cannot guarantee which one is the last value and we return null in order to avoid misunderstandings.
                return new ParameterResolverValue(null);
            } else {
                var count = args.Length / 2;
                
                for (int i = 0; i < count; i++) {
                    var name = args[i * 2].Value as string;
                    var value = args[i * 2 + 1].Value;
                    if (name != null) {
                        result[name] = value;
                        lastvalue = value;
                    } else {
                        throw new ArgumentException($"SetResult works with no arguments or with even number of arguments repeatig name, value pattern. The {i * 2} argument is not a string.");
                    }
                }
                return new ParameterResolverValue(lastvalue);
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
                            throw new ArgumentException("The argument of ModulePath, if supplied, must contain path without .. and not starting with a slash");
                        }
                        return new ParameterResolverValue(Path.Combine(path, subpath));
                    }
                } else
                {
                    return new ParameterResolverValue(path);
                }
            } else
            {
                throw new Exception("Cannot obtain the context or the global settings.");
            }
        }


        public ParameterResolverValue ModuleName(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            return new ParameterResolverValue(ctx.ProcessingContext.InputModel.Module);
        }
        public ParameterResolverValue NodePath(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            return new ParameterResolverValue(ctx.ProcessingContext.InputModel.Nodepath);
            
        }
        public ParameterResolverValue NodeKey(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            return new ParameterResolverValue(ctx.NodeKey);
        }
        public ParameterResolverValue Action(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            return new ParameterResolverValue(ctx.Action);
        }
        public ParameterResolverValue Operation(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            return new ParameterResolverValue(ctx.Operation);
        }
        #endregion

        #region Settings
        public ParameterResolverValue CSetting(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as IDataLoaderContext;
            if (args.Length < 1 || args.Length > 2) {
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
            } else
            {
                return new ParameterResolverValue(null);
            }
        }
        
        #endregion
    }
}
