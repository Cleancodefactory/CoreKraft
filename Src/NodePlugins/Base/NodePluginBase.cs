using Ccf.Ck.SysPlugins.Interfaces;
using System;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;


namespace Ccf.Ck.NodePlugins.Base
{
    public abstract class NodePluginBase<ScopeContext> : NodePluginProtoBase<ScopeContext>
        where ScopeContext: class, IPluginsSynchronizeContextScoped, new() {

            public NodePluginBase() { }
            public override void Execute(INodePluginContext p) {
                if (p.Action == ACTION_READ) {
                    ExecuteRead(p as INodePluginReadContext);
                } else if (p.Action == ACTION_WRITE) {
                    ExecuteWrite(p as INodePluginWriteContext);
                } else {
                    throw new Exception("Unknown action: " + (p.Action ?? "null"));
                }
            }

        #region Overridables
        protected abstract void ExecuteRead(INodePluginReadContext pr);
        protected abstract void ExecuteWrite(INodePluginWriteContext pw);
        #endregion
    }
}
