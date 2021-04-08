using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.NodePlugins.Base
{
    public abstract class NodePluginProtoBase<ScopeContext>: INodePlugin
        where ScopeContext : class, IPluginsSynchronizeContextScoped, new()
    {
        public abstract void Execute(INodePluginContext p);

        public virtual Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()
        {
            return Task<IPluginsSynchronizeContextScoped>.FromResult(new ScopeContext() as IPluginsSynchronizeContextScoped);
        }

        #region Helpers
        protected ActionBase Action(IDataLoaderContext execContext)
        {
            if (execContext.Action == ACTION_READ) {
                return execContext.CurrentNode.Read.Select;
            } else if (execContext.Action == ACTION_WRITE) {
                switch (execContext.Operation) {
                    case OPERATION_INSERT:
                        return execContext.CurrentNode.Write.Insert;
                    case OPERATION_UPDATE:
                        return execContext.CurrentNode.Write.Update;
                    case OPERATION_DELETE:
                        return execContext.CurrentNode.Write.Delete;
                }
            }
            return null;
        }
        protected void ApplyResultsToRow(Dictionary<string, object> row, IEnumerable<Dictionary<string, object>> results)
        {
            if (row != null && results != null) {
                foreach (var result in results) {
                    if (result != null) {
                        foreach (var kvp in result) {
                            row[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
        }
        protected ScopeContext Scope(INodePluginContext p) { return p.OwnContextScoped as ScopeContext; }
        protected void UpdateRow(Dictionary<string, object> result, INodePluginWriteContext writectx)
        {
            if (writectx.Row != null) {
                foreach (var kvp in result) {
                    writectx.Row[kvp.Key] = kvp.Value;
                }
            }
        }
        #endregion
    }
}
