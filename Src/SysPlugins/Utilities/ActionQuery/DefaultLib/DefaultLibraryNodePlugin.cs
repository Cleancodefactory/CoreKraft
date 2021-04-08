using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class DefaultLibraryNodePlugin<HostInterface>: DefaultLibraryBase<HostInterface> where HostInterface: class
    {
        public DefaultLibraryNodePlugin()
        {

        }
        public static DefaultLibraryNodePlugin<HostInterface> Instance { get; private set; }

        private class __Creator
        {
            static __Creator()
            {
                DefaultLibraryNodePlugin<HostInterface>.Instance = new DefaultLibraryNodePlugin<HostInterface>();
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

                case nameof(AddResult):
                    return AddResult;
                case nameof(HasResults):
                    return HasResults;
                case nameof(SetResult):
                    return SetResult;
            }
            return base.GetProc(name);
        }

        #region

        public ParameterResolverValue AddResult(HostInterface ctx, ParameterResolverValue[] args)
        {
            if (ctx is INodePluginReadContext rctx)
            {
                var result = new Dictionary<string, object>();
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
            else
            {
                throw new InvalidOperationException("AddResult can be used only in Read actions");
            }
        }
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
        #endregion

        #region Information about what is happening
        public ParameterResolverValue NodePath(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
            return new ParameterResolverValue(ctx.Path);
            
        }
        public ParameterResolverValue NodeKey(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
            return new ParameterResolverValue(ctx.NodeKey);
        }
        public ParameterResolverValue Action(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
            return new ParameterResolverValue(ctx.Action);
        }
        public ParameterResolverValue Operation(HostInterface _ctx, ParameterResolverValue[] args)
        {
            var ctx = _ctx as INodePluginContext;
            return new ParameterResolverValue(ctx.Operation);
        }
        #endregion
    }
}
