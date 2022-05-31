using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.NodePlugins.Base
{
    public abstract class NodePluginClassicBase<ScopeContext>: NodePluginProtoBase<ScopeContext>
        where ScopeContext : class, IPluginsSynchronizeContextScoped, new()
    {
        public NodePluginClassicBase() { }
        public override void Execute(INodePluginContext p)
        {
            object results;
            if (p.Action == ACTION_READ) {
                var readctx = p as INodePluginReadContext;
                results = Read(readctx);
                if (results is Dictionary<string, object>) {
                    readctx.Results.Add(results as Dictionary<string, object>);
                } else if (results is IEnumerable<Dictionary<string, object>>) {
                    foreach (var result in results as IEnumerable<Dictionary<string, object>>) {
                        readctx.Results.Add(result);
                    }
                } else {
                    throw new Exception("Unexpected type returned by ExecuteRead");
                }
            } else if (p.Action == ACTION_WRITE) {
                var writectx = p as INodePluginWriteContext;
                results = Write(writectx);
                if (results is Dictionary<string, object>) {
                    UpdateRow(results as Dictionary<string, object>, writectx);
                } else if (results is IEnumerable<Dictionary<string, object>>) {
                    foreach (var result in results as IEnumerable<Dictionary<string, object>>) {
                        UpdateRow(result, writectx);
                    }
                } else {
                    throw new Exception("Unexpected type returned by ExecuteRead");
                }
            } else {
                throw new Exception("Unknown action: " + (p.Action ?? "null"));
            }
        }

        #region Overridables
        protected abstract object Read(INodePluginReadContext pr);
        protected abstract object Write(INodePluginWriteContext pw);
        #endregion
    }
}
