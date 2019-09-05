using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Resolvers;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;

namespace Ccf.Ck.SysPlugins.Data.Db.ADO
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="FieldTypeEnum">The enumeration to use in field types</typeparam>
    public abstract class ADOBase<XConnection> : DataLoaderClassicBase<ADOSynchronizeContextScopedDefault<XConnection>> // IDataLoaderPlugin
        where XConnection : DbConnection, new()
    {

        #region Construction and feature configuration
        protected bool SupportTableParameters { get; set; } = false;
        public ADOBase() { }
        #endregion

        #region Configuration reading and peparation
        /*  Considerations:
         *      We have to read the configuration in a single turn and consume it from the internal configuration container
         *      in order to avoid direct hard coded dependency on the system configuration structure - most notably dependency on its
         *      structure and interpreatation.
         *  Solution:
         *      We have a nested class in which the configuration values used by this loader are collected (sometimes with some preprocessing)
         *  Lifecycle:
         *      The configuration (part of it at least) has a lifecycle equal to the node execution which is shorter than the life of the dataloader (at least potentially),
         *      so the configuration has to be reread on each node executio and thus is not persisted in the loader's instance, but passed to the main overridable methods.
         * 
         */
        //protected class Configuration {
        //    public string Statement { get; set; }

        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execContext"></param>
        /// <returns></returns>
        //protected virtual Configuration ReadConfiguration(IDataLoaderContext execContext) {
        //    var cfg = new Configuration();
            
        //    if (execContext.Action == ACTION_READ) {
        //        cfg.Statement = execContext.CurrentNode.Read.Select.HasStatement?execContext.CurrentNode.Read.Select.Query:null;
        //    } else if (execContext.Action == ACTION_WRITE) {
        //        switch (execContext.Operation) {
        //            case OPERATION_INSERT:
        //                if (execContext.CurrentNode.Write.Insert.HasStatement) {
        //                    cfg.Statement = execContext.CurrentNode.Write.Insert.Query;
        //                }
        //                break;
        //            case OPERATION_UPDATE:
        //                if (execContext.CurrentNode.Write.Update.HasStatement) {
        //                    cfg.Statement = execContext.CurrentNode.Write.Update.Query;
        //                }
        //                break;
        //            case OPERATION_DELETE:
        //                if (execContext.CurrentNode.Write.Delete.HasStatement) {
        //                    cfg.Statement = execContext.CurrentNode.Write.Delete.Query;
        //                }
        //                break;
        //        }
        //    }
        //    return cfg; ///////////////////////////////
        //}
        #endregion


        //public async Task<object> ExecuteAsync(IDataLoaderContext execContext) {
        //    // The procedure is different enough to deserve splitting by read/write
        //    Configuration cfg = ReadConfiguration(execContext);
        //    object result;
        //    if (execContext.Action == ACTION_WRITE) {
        //        result = ExecuteWrite(execContext, cfg);
        //    } else if (execContext.Action == ACTION_READ) {
        //        result = ExecuteRead(execContext, cfg);
        //    } else {
        //        // unknown action
        //        throw new Exception("Unknown action (only read/write) are supported");
        //    }
        //    return await Task.FromResult(result);
        //}

        protected override List<Dictionary<string,object>> Read(IDataLoaderReadContext execContext) {
            // TODO: What to return if there is no statement:
            //  I think we should have two policies - empty object which enables children extraction if logically possible and
            //  null wich stops the processing here.
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>(); 
            Dictionary<string, object> currentResult = null;
            Node node = execContext.CurrentNode;

            if (!string.IsNullOrWhiteSpace(Action(execContext).Query)) {
                // Scope context for the same loader
                IADOTransactionScope scopedContext = execContext.OwnContextScoped as IADOTransactionScope;
                // Check it is valid
                if (scopedContext == null) {
                    throw new NullReferenceException("Scoped synchronization and transaction context is not available.");
                }
                // Configuration settings Should be set to the scoped context during its creation/obtainment - see ExternalServiceImp

                // No tranaction in read mode - lets not forget that closing the transaction also closes the connection - so the ;ifecycle control will do this using the transaction based notation
                // from ITransactionScope
                DbConnection conn = scopedContext.Connection;
                using (DbCommand cmd = conn.CreateCommand()) {
                    cmd.Transaction = scopedContext.CurrentTransaction; // if we decide to open transaction in future this will guarantee we only have to open it and will take effect throughout the code.
                    cmd.Parameters.Clear();
                    // This will set the resulting command text if everything is Ok.
                    // The processing will make replacements in the SQL and bind parameters by requesting them from the resolver expressions configured on this node.
                    // TODO: Some try...catching is necessary.
                    ProcessCommand(cmd, Action(execContext).Query, execContext);
                    using (DbDataReader reader = cmd.ExecuteReader()) {
                        do {
                            if (reader.HasRows) {
                                // Read a result (many may be contained) row by row
                                while (reader.Read()) {
                                    currentResult = new Dictionary<string, object>(reader.FieldCount);
                                    for (int i = 0; i < reader.FieldCount; i++) {
                                        string fldname = reader.GetName(i);
                                        if (fldname == null) continue;
                                        // TODO: May be configure that or at least create a compile time definition
                                        fldname = fldname.ToLower().Trim(); // TODO: lowercase
                                        //fldname = fldname.Trim();
                                        if (fldname.Length == 0) {
                                            throw new Exception($"Empty name when reading the output of a query. The field index is {i}. The query is: {cmd.CommandText}");
                                        }
                                        if (currentResult.ContainsKey(fldname)) {
                                            throw new Exception($"Duplicated field name in the output of a query. The field is:{fldname}, the query is: {cmd.CommandText}");
                                        }
                                        object v = reader.GetValue(i);
                                        currentResult.Add(fldname, (v is DBNull) ? null : v);

                                    }
                                    // Mark the records unchanged, because they are just picked up from the data store (rdbms in this case).
                                    execContext.DataState.SetUnchanged(currentResult);
                                    results.Add(currentResult);
                                    if (!node.IsList) break;

                                }
                            }
                        } while (reader.NextResult());
                    }

                }
            }
            return results; // TODO: Decide what behavior we want with empty statements. I for one prefer null result, effectively stopping the operation.
        }
        /// <summary>
        /// Unlike read write is called exactly once per each row and not called at all if the row's state does not require actual writing.
        /// </summary>
        /// <param name="execContext"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        protected override object Write(IDataLoaderWriteContext execContext) { //IDataLoaderContext execContext, Configuration configuration) {
            Node node = execContext.CurrentNode;

            // Statement is already selected for the requested operation (While fetching the Configuration
            if (!string.IsNullOrWhiteSpace(Action(execContext).Query)) {
                IADOTransactionScope scopedContext = execContext.OwnContextScoped as IADOTransactionScope;
                // Check if it is valid
                if (scopedContext == null) {
                    throw new NullReferenceException("Scoped synchronization and transaction context is not available.");
                }
                // Settings should be passed to the scopedContext in the ExternalServiceImp
                DbConnection conn = scopedContext.Connection;
                DbTransaction trans = scopedContext.StartADOTransaction();

                using (DbCommand cmd = conn.CreateCommand()) {
                    cmd.Transaction = trans;
                    cmd.Parameters.Clear();
                    ProcessCommand(cmd, Action(execContext).Query, execContext);

                    using (DbDataReader reader = cmd.ExecuteReader()) {
                        do {
                            while (reader.Read()) {
                                for (int i = 0; i < reader.FieldCount; i++) {
                                    string fname = reader.GetName(i);
                                    if (fname == null) continue;
                                    fname = fname.ToLower().Trim();
                                    // fname = fname.Trim(); // TODO: We have to rethink this - lowercasing seems more inconvenience than a viable protection against human mistakes.
                                    if (fname.Length == 0) throw new Exception("Empty field name in a store context in nodedesfinition: " + node.NodeSet.Name);
                                    object v = reader.GetValue(i);
                                    execContext.Row[fname] = (v is DBNull) ? null : v;
                                }
                            }
                            // This is important, we have been doing this for a single result before, but it is better to assume more than one, so that 
                            // update of the data being written can be done more freely - using more than one select statement after writing. This is
                            // probably rare, but having the opportunity is better than not having it.
                        } while (reader.NextResult());
                        if (execContext.Operation != OPERATION_DELETE) {
                            execContext.DataState.SetUnchanged(execContext.Row);
                        }
                    }
                }
            }
            return null; // if this is not null it should add new results in the data
            // TODO: Consider if this is possible and useful (for some future version - not urgent).
        }

        //public abstract Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync();

        #region base functionality
        /// <summary>
        /// This method is called to bind the parameter over the command. While the default implementation does so, the name parameter 
        /// does not need to match the name specified in the SQL - DataLoaders may choose to implement more complex processing where the 
        /// actual parameter names used need to be re/generated separately and are never seen outside of the DataLoader's code.
        /// </summary>
        /// <param name="cmd">Command to bind to - to keep this imutable it is passed as parameter</param>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">ParameterResolverValue - the actual value has to be packed in this type</param>
        protected virtual void BindParameter(DbCommand cmd,string name, ParameterResolverValue? value) {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("The parameter name needs to be non-null and non-empty");
            if (cmd == null) throw new ArgumentNullException("cmd cannot be null");
            // Unlike the others missing value is simple threated as DbNull.
            if (!cmd.Parameters.Contains(name))
            {
                var param = cmd.CreateParameter();
                param.ParameterName = name;
                if (value == null)
                {
                    // implicit null
                    param.Value = DBNull.Value;
                }
                else
                {
                    if (value.Value.Value == null)
                    {
                        param.Value = DBNull.Value;
                    }
                    else
                    {
                        // Different providers may sometimes use different properties and specific enums to specify the type.
                        // Additional measures may be needed as well, so we leave the operation in an overridable method.
                        AssignParameterType(param, GetParameterType(value.Value));
                        param.Value = value.Value.Value;
                    }
                }
                cmd.Parameters.Add(param);
            }
            
        }
        /// <summary>
        /// Guesses/determines appropriate database type for the value. Although this is done over a value it is usually the same 
        /// for the corresponding field (if there is such). It is not determined in some more static manner, because we use resolvers
        /// and they record their result with the value. It is up to them to suggest a type dynamically or statically (the same for the 
        /// same field, no matter what the actual data is). This way the developer can choose a resolver and pass parameters to it so
        /// that the desired behavior can be achieved. Lets not forget that we have this situation with DataLoaders that work with 
        /// non-RDBMS storages - from outside the way we deal with value typing should follow the same patterns and use the same kind
        /// of instruments.
        /// </summary>
        /// <param name="value">The value for which to suggest type</param>
        /// <returns>Returns the type from the appropriate enum or set of constants casted to int in order to avoid the unneeded boxing. Int fits all! If no type can be
        /// suggested null should be returned and dealt appropriately in BindParameter.</returns>
        protected virtual int? GetParameterType(ParameterResolverValue value) {
            // Base behavior - say nothing, everything goes default.
            // In more specialized classes here is the place to perform mapping and/or type selection according to the application architectural concepts.
            return null;
        }
        /// <summary>
        /// Implement this to assign to a more specific property if needed. int? is used because no matter the provider int? is enogh to represent the
        /// type value used by it.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="type"></param>
        protected virtual void AssignParameterType(DbParameter param, int? type) {
            if (type.HasValue) {
                param.DbType = (DbType)type.Value;
            } else {
                param.ResetDbType();
            }
        }
        /// <summary>
        /// Performs the standard processing - extract parameters, replacing those that resolve to literals (ValueType == ContentType)
        /// and binding to the command the ValueType ones.
        /// To avoid potential discrepancies with DbCommand implementations the SQL is supplied separately and set only once to the command.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="sql"></param>
        /// <param name="execContext"></param>
        protected virtual void ProcessCommand(DbCommand cmd, string sql, IDataLoaderContext execContext) {
            if (string.IsNullOrWhiteSpace(sql)) return; // We just do nothing in this case.
            Regex reg = new Regex(@"@([a-zA-Z_][a-zA-Z0-9_\$]*)",RegexOptions.None); // This should be configurable

            string result = reg.Replace(sql, m => {
                if (m.Groups[1].Success) {
                    var paramname = m.Groups[1].Value;
                    // Evaluate the parameter
                    var v = execContext.Evaluate(paramname);
                    if ((int)v.ValueType >= 0)
                    {
                        switch (v.ValueType)
                        {
                            case EResolverValueType.ValueType:
                                if ((v.ValueDataType & EValueDataType.Collection) != 0 && SupportTableParameters == false)
                                {
                                    throw new Exception("Loader doesn't support Table-Valued Parameters");
                                }
                                // Bind value
                                BindParameter(cmd, paramname, v);
                                return "@" + paramname;
                            case EResolverValueType.Skip:
                                return "@" + paramname;
                            default:
                                // Replace text
                                // We ignore all kinds of type info and we just use ToString();
                                string x = v.Value == null ? "" : v.Value.ToString();
                                return x;
                        }
                    }// else skip the rest and leave them unbound. TODO: We will see if this is the correct default behavior.
                } else {
                    throw new Exception("While parsing SQL we received match with invalid first group (which should be impossible)");
                }
                return m.Value;
            });
            cmd.CommandText = result;
        }
        #endregion
    }

    //public class ADOLoader<SyncScope>: ADOBase where SyncScope: IPluginsSynchronizeContextScoped, ITransactionScope, new() {
    //    public async override Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync() {
    //        return await Task.FromResult(new SyncScope());
    //    }
    //}

}
