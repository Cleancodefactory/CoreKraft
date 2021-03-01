﻿using System;
using Ccf.Ck.SysPlugins.Interfaces;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.SysPlugins.Data.Base
{
    public abstract class DataLoaderBase<ScopeContext> : DataLoaderProtoBase<ScopeContext> where ScopeContext : class, IPluginsSynchronizeContextScoped, new()
    {
        public DataLoaderBase() { }

        public override void Execute(IDataLoaderContext execContext)
        {
            // TODO: Needs a bit more effort to pack exception from the called methods
            if (execContext.Action == ACTION_READ)
            {
                ExecuteRead(execContext as IDataLoaderReadContext);
            }
            else if (execContext.Action == ACTION_WRITE)
            {
                if (execContext is IDataLoaderWriteContext) {
                    ExecuteWrite(execContext as IDataLoaderWriteContext);
                } 
                else if (execContext is IDataLoaderWriteAppendContext) {
                    ExecuteWriteAppend(execContext as IDataLoaderWriteAppendContext);
                }                
            }
            else
            {
                throw new Exception("Unknown action " + (execContext.Action ?? "null"));
            }
        }

        #region overridables
        protected abstract void ExecuteRead(IDataLoaderReadContext execContext);
        protected abstract void ExecuteWrite(IDataLoaderWriteContext execContext);
        protected virtual void ExecuteWriteAppend(IDataLoaderWriteAppendContext execContext) {
            throw new Exception($"This plugin ( {this.GetType().FullName} ) does not support AppendResults in write actions. ");
        }
        #endregion

    }
}
