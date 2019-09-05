using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Lookups.ADO
{
    public abstract class LookupADOBase : ISystemPlugin
    {
        public async Task<IProcessingContext> ExecuteAsync(
            LoadedNodeSet loaderContext, 
            IProcessingContext processingContext, 
            IPluginServiceManager pluginServiceManager, 
            IPluginsSynchronizeContextScoped contextScoped, 
            INode currentNode)
        {
            processingContext.ReturnModel.LookupData = 
                LoadLookups(loaderContext, processingContext, pluginServiceManager, contextScoped, currentNode);
            return await Task.FromResult(processingContext);
        }

        protected virtual Dictionary<string,object> LoadLookups(
            LoadedNodeSet loaderContext,
            IProcessingContext processingContext,
            IPluginServiceManager pluginServiceManager,
            IPluginsSynchronizeContextScoped contextScoped,
            INode currentNode)
        {
            Dictionary<string, object> returnResult = new Dictionary<string, object>();
            List<Dictionary<string, object>> results = null;
            Dictionary<string, object> currentResult = null;

            // This shouldn't happen to a dog! but I needed a hack for basket consumer.
            LookupLoaderContext.Instance.PluginServiceManager = pluginServiceManager;
            LookupLoaderContext.Instance.LoaderContext = loaderContext;
            LookupLoaderContext.Instance.ProcessingContext = processingContext;
            //
            
            Lookup lookup = (Lookup)currentNode;
            if (lookup.HasStatement())
            {
                results = new List<Dictionary<string, object>>();                

                IADOTransactionScope scopedContext = contextScoped as IADOTransactionScope;
                // Check it is valid

                if (scopedContext == null)
                {
                    throw new NullReferenceException("Scoped synchronization and transaction context is not available.");
                }

                DbConnection conn = scopedContext.Connection;
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = lookup.Query;
                    using(DbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                currentResult = new Dictionary<string, object>(reader.FieldCount);
                                currentResult = new Dictionary<string, object>(reader.FieldCount);
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    string fldname = reader.GetName(i);
                                    if (fldname == null) continue;
                                    fldname = fldname.ToLower().Trim();
                                    if (fldname.Length == 0)
                                    {
                                        throw new Exception($"Empty name when reading the output of a query. The field index is {i}. The query is: {cmd.CommandText}");
                                    }
                                    if (currentResult.ContainsKey(fldname))
                                    {
                                        throw new Exception($"Duplicated field name in the output of a query. The field is:{fldname}, the query is: {cmd.CommandText}");
                                    }
                                    object o = reader.GetValue(i);
                                    currentResult.Add(fldname, (o is DBNull) ? null : o);
                                }
                                results.Add(currentResult);
                            }
                        }
                    }
                }
                returnResult.Add(lookup.BindingKey, results);
            }
            return returnResult;
        }

        public abstract Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync();
    }
}
