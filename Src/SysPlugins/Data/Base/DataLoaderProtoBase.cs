using System.Collections.Generic;
using System.Threading.Tasks;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.NodeSet;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.SysPlugins.Data.Base
{
    /// <summary>
    /// This class is not designed as a typical Base class for implementation of DataLoader-s. While this is Ok for untypical DataLoaders, normally one should inherit
    /// from DataLoaderBase or DataLoaderClassicBase
    /// </summary>
    /// <typeparam name="ScopeContext"></typeparam>
    public abstract class DataLoaderProtoBase<ScopeContext> : IDataLoaderPlugin where ScopeContext : class, IPluginsSynchronizeContextScoped, new()
    {
        #region IDataLoaderPlugin
        public abstract void Execute(IDataLoaderContext execContext);

        public virtual Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync()  {
            return Task<IPluginsSynchronizeContextScoped>.FromResult(new ScopeContext() as IPluginsSynchronizeContextScoped);
        }
        #endregion

        #region Helpers
        protected ScopeContext Scope(IDataLoaderContext p) { return p.OwnContextScoped as ScopeContext; }
        protected ActionBase Action(IDataLoaderContext execContext)  {
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
        protected void ApplyResultsToRow(Dictionary<string, object> row, IEnumerable<Dictionary<string, object>> results) {
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
        #endregion
    }
}
