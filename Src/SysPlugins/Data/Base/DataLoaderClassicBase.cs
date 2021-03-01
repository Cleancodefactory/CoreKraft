using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.SysPlugins.Data.Base
{
    public abstract class DataLoaderClassicBase<ScopeContext> : DataLoaderBase<ScopeContext> where ScopeContext : class, IPluginsSynchronizeContextScoped, new()
    {
        public DataLoaderClassicBase() { }

        #region IDataLoaderPlugin

        public override void Execute(IDataLoaderContext execContext)
        {
            // TODO: Needs a bit more effort to pack exception from the called methods
            if (execContext.Action == ACTION_READ)
            {
                ExecuteRead(execContext as IDataLoaderReadContext);
            }
            else if (execContext.Action == ACTION_WRITE)
            {
                if (execContext is IDataLoaderWriteContext)
                {
                    ExecuteWrite(execContext as IDataLoaderWriteContext);
                }
                else if (execContext is IDataLoaderWriteAppendContext)
                {
                    ExecuteWriteAppend(execContext as IDataLoaderWriteAppendContext);
                }
            }
            else
            {
                throw new Exception("Unknown action " + (execContext.Action ?? "null"));
            }
        }

        protected override void ExecuteRead(IDataLoaderReadContext execContext)
        {
            var results = Read(execContext);
            if (results != null && results.Count > 0)
            {
                execContext.Results.AddRange(results);
            }
        }

        protected override void ExecuteWrite(IDataLoaderWriteContext execContext)
        {
            var u = Write(execContext);
            if (u != null)
            {
                if (u is IDictionary<string, object>)
                {
                    ApplyResultsToRow(execContext.Row, new List<Dictionary<string, object>>() { u as Dictionary<string, object> });
                }
                else if (u is IEnumerable<Dictionary<string, object>>)
                {
                    ApplyResultsToRow(execContext.Row, u as IEnumerable<Dictionary<string, object>>);
                }
                else
                {
                    throw new InvalidCastException("Write method is expected to return either one Dictionary<string, object> or IEnumerable of them.");
                }
            }
        }
        protected override void ExecuteWriteAppend(IDataLoaderWriteAppendContext execContext) {
            object u = WriteAppend(execContext);
            if (u != null)
            {
                if (u is IDictionary<string, object>)
                {
                    execContext.AppendResults.Add(u as Dictionary<string, object>);
                }
                else if (u is IEnumerable<Dictionary<string, object>>)
                {
                    execContext.AppendResults.AddRange(u as IEnumerable<Dictionary<string, object>>);
                }
                else
                {
                    throw new InvalidCastException("Write method is expected to return either one Dictionary<string, object> or IEnumerable of them.");
                }
            }
        }
        #endregion IDataLoaderPlugin

        #region Overridables
        protected abstract List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext);
        protected abstract object Write(IDataLoaderWriteContext execContext);
        protected virtual object WriteAppend(IDataLoaderWriteAppendContext execContext) {
            throw new Exception($"This plugin ( {this.GetType().FullName} ) does not support AppendResults in write actions. ");
        }
        #endregion
    }
}
