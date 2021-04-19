using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.ContextBasket;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Utilities;
using System;

namespace Ccf.Ck.SysPlugins.Data.Scripter
{
    public class ScripterImp : DataLoaderBase<ScripterSynchronizeContextScopedImp>
    {

        #region DataLoaderBase
        protected override void ExecuteRead(IDataLoaderReadContext execContext)
        {
            ExecuteQuery(execContext);
        }

        protected override void ExecuteWrite(IDataLoaderWriteContext execContext)
        {
            ExecuteQuery(execContext);
        }

        protected virtual void ExecuteQuery<Context>(Context execContext) where Context: class, IDataLoaderContext
        {
            string qry = GetQuery(execContext);
            if (qry != null)
            {
                try
                {
                    var runner = Compiler.Compile(qry);
                    if (runner.ErrorText != null)
                    {
                        
                        throw new Exception(runner.ErrorText);
                    }
                    var host = new ActionQueryHost<Context>(execContext)
                    {
                        { "HostInfo", HostInfo }
                    };

                    var result = runner.ExecuteScalar(host);
                } 
                catch (Exception ex)
                {
                    
                    KraftLogger.LogError("@@@", ex);
                    if (ex.InnerException != null)
                    {
                        throw ex.InnerException;
                    }
                }
            }
        }
        #endregion

        #region Helpers

        private string GetQuery(IDataLoaderContext ctx)
        {
            if (ctx.Action == ModelConstants.ACTION_READ)
            {
                var op = ctx.CurrentNode?.Read?.Select;
                if (op != null)
                {
                    return op.Query;
                }
            }
            else if (ctx.Action == ModelConstants.ACTION_WRITE)
            {
                switch (ctx.Operation)
                {
                    case ModelConstants.OPERATION_INSERT:
                        return ctx.CurrentNode?.Write?.Insert?.Query;
                    case ModelConstants.OPERATION_UPDATE:
                        return ctx.CurrentNode?.Write?.Update?.Query;
                    case ModelConstants.OPERATION_DELETE:
                        return ctx.CurrentNode?.Write?.Delete?.Query;
                }
            }
            return null;
        }
        #endregion

        public ParameterResolverValue HostInfo(IDataLoaderContext ctx, ParameterResolverValue[] args)
        {
            return new ParameterResolverValue("Scripter 1.0");
        }
    }
}
