using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.SysPlugins.Data.Base;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using static Ccf.Ck.Models.ContextBasket.ModelConstants;
using Ccf.Ck.SysPlugins.Interfaces.NodeExecution;
using static System.Net.Mime.MediaTypeNames;

namespace Ccf.Ck.SysPlugins.Data.Db.ADO
{
    /// <summary>
    /// Introduced as a middleground to enable definition of DB specific scoped contexts
    /// </summary>
    /// <typeparam name="XConnection"></typeparam>
    /// <typeparam name="ScopeContext"></typeparam>
    public abstract class ADOProtoBase<XConnection, ScopeContext> : DataLoaderClassicBase<ScopeContext>, IDataLoaderPluginPrepare where ScopeContext : class, IPluginsSynchronizeContextScoped, new()
        where XConnection : DbConnection, new()
    {

        #region Construction and feature configuration
        protected bool SupportTableParameters { get; set; } = false;
        protected int max_recursions = 0;
        public const string MAX_RECURSIONS_NAME = "max_recursions";
        public const int MAX_RECURSIONS_DEFAULT = 5;
        public ADOProtoBase() { }
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

        #region Prepare
        public virtual void Prepare(IDataLoaderReadContext execContext) {
            ADOInfo metaReport = null;
            if (execContext is IActionHelpers helper) {
                metaReport = helper.NodeMeta.CreateInfo<ADOInfo>();
            }
            // TODO Review the meta dara - we may want to add Prepare specific entry

            // TODO: What to return if there is no statement:
            //  I think we should have two policies - empty object which enables children extraction if logically possible and
            //  null wich stops the processing here.
            List<Dictionary<string, object>> results = execContext.Results;
            string sqlQuery = null;

            List<string> parameters = new List<string>();
            try {
                Node node = execContext.CurrentNode;
                string query = null;
                if (execContext.Action == ACTION_READ) {
                    query = node.Read?.Prepare?.Query;
                } else if (execContext.Action == ACTION_WRITE) {
                    query = node.Write?.Prepare?.Query;
                }

                if (!string.IsNullOrWhiteSpace(query)) {
                    // Scope context for the same loader
                    // Check it is valid
                    if (!(execContext.OwnContextScoped is IADOTransactionScope scopedContext)) {
                        throw new NullReferenceException("Scoped synchronization and transaction context is not available.");
                    }
                    // Configuration settings Should be set to the scoped context during its creation/obtainment - see ExternalServiceImp

                    // No tranaction in read mode - lets not forget that closing the transaction also closes the connection - so the ;ifecycle control will do this using the transaction based notation
                    // from ITransactionScope
                    DbConnection conn = scopedContext.Connection;
                    using (DbCommand cmd = conn.CreateCommand()) {
                        //cmd.Transaction = scopedContext.CurrentTransaction; //no transaction
                        if (execContext.Action == ACTION_READ && scopedContext is ADOSynchronizeContextScopedDefault<XConnection> scopedDefault) {
                            if (scopedDefault.IsStartReadTransaction()) {
                                cmd.Transaction = scopedContext.StartADOTransaction(); //start transaction
                            }
                        }

                        cmd.Parameters.Clear();
                        
                        sqlQuery = ProcessCommand(cmd, query, execContext, out parameters);
                        if (metaReport != null) {
                            metaReport.ReportSQL(sqlQuery);
                            metaReport.ReportParameters(cmd.Parameters);
                        }
                        LogExecution(sqlQuery, execContext, parameters);
                        using (DbDataReader reader = cmd.ExecuteReader()) {
                            do {
                                // TODO how to proceed here - side effects or no sideffects are we going to return something?
                                int n_rows = 0;
                                if (reader.HasRows) {
                                    // We do not use non-result execution, because we are not sure if we are going to allow Results changes in a new version
                                    while (reader.Read()) {
                                        n_rows++;
                                    }
 
                                }
                                if (metaReport != null) {
                                    metaReport.AddResult(n_rows, reader.FieldCount);
                                }
                            } while (reader.NextResult());
                            reader.Close();
                            if (metaReport != null) {
                                metaReport.RowsAffected = reader.RecordsAffected;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                if (Action(execContext) == null) {
                    throw new Exception($"Missing action: {execContext.Action}, operation: {execContext.Operation} for node: {execContext.CurrentNode.NodeKey} (Module: {execContext.ProcessingContext.InputModel.Module})");
                }
                StringBuilder sbError = new StringBuilder(1000);
                sbError.AppendLine($"Prepare in action: {execContext.Action}, operation: {execContext.Operation} for node: {execContext.CurrentNode.NodeKey} (Module: {execContext.ProcessingContext.InputModel.Module}");
                if (!string.IsNullOrEmpty(sqlQuery)) {
                    StringBuilder sb = new StringBuilder();
                    foreach (string param in parameters) {
                        sb.AppendLine(param);
                    }
                    sbError.AppendLine($"Read(IDataLoaderReadContext execContext) >> SQL: {sb.ToString()}{Environment.NewLine}{sqlQuery}");
                }
                KraftLogger.LogError(sbError.ToString(), ex, execContext);
                metaReport.SetErrorInfo(ex, sbError.ToString());
                throw;
            }
        }

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

        protected override List<Dictionary<string, object>> Read(IDataLoaderReadContext execContext)
        {
            ADOInfo metaReport = null;
            if (execContext is IActionHelpers helper) {
                metaReport = helper.NodeMeta.CreateInfo<ADOInfo>();
            }
            var metaNode = execContext as IActionHelpers;

            // TODO: What to return if there is no statement:
            //  I think we should have two policies - empty object which enables children extraction if logically possible and
            //  null wich stops the processing here.
            List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
            string sqlQuery = null;
            List<string> parameters = new List<string>();
            try
            {
                Node node = execContext.CurrentNode;

                if (!string.IsNullOrWhiteSpace(Action(execContext).Query))
                {
                    // Scope context for the same loader
                    // Check it is valid
                    if (!(execContext.OwnContextScoped is IADOTransactionScope scopedContext))
                    {
                        throw new NullReferenceException("Scoped synchronization and transaction context is not available.");
                    }
                    // Configuration settings Should be set to the scoped context during its creation/obtainment - see ExternalServiceImp

                    // No tranaction in read mode - lets not forget that closing the transaction also closes the connection - so the ;ifecycle control will do this using the transaction based notation
                    // from ITransactionScope
                    DbConnection conn = scopedContext.Connection;
                    using (DbCommand cmd = conn.CreateCommand())
                    {
                        //cmd.Transaction = scopedContext.CurrentTransaction; //no transaction
                        if (scopedContext is ADOSynchronizeContextScopedDefault<XConnection> scopedDefault)
                        {
                            if (scopedDefault.IsStartReadTransaction())
                            {
                                cmd.Transaction = scopedContext.StartADOTransaction(); //start transaction
                            }
                        }
                        
                        cmd.Parameters.Clear();
                        // This will set the resulting command text if everything is Ok.
                        // The processing will make replacements in the SQL and bind parameters by requesting them from the resolver expressions configured on this node.
                        // TODO: Some try...catching is necessary.
                        sqlQuery = ProcessCommand(cmd, Action(execContext).Query, execContext, out parameters);
                        if (metaReport != null) {
                            metaReport.ReportSQL(sqlQuery);
                            metaReport.ReportParameters(cmd.Parameters);
                        }
                        LogExecution(sqlQuery, execContext, parameters);
                        using (DbDataReader reader = cmd.ExecuteReader())
                        {
                            do
                            {
                                int n_rows = 0;
                                if (reader.HasRows)
                                {
                                    // Read a result (many may be contained) row by row
                                    while (reader.Read())
                                    {
                                        n_rows++;
                                        Dictionary<string, object> currentResult = new Dictionary<string, object>(reader.FieldCount);
                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            string fldname = reader.GetName(i);
                                            if (fldname == null) continue;
                                            // TODO: May be configure that or at least create a compile time definition
                                            fldname = fldname.ToLower().Trim(); // TODO: lowercase
                                                                                //fldname = fldname.Trim();
                                            if (fldname.Length == 0)
                                            {
                                                throw new Exception($"Empty name when reading the output of a query. The field index is {i}. The query is: {cmd.CommandText}");
                                            }
                                            if (currentResult.ContainsKey(fldname))
                                            {
                                                throw new Exception($"Duplicated field name in the output of a query. The field is:{fldname}, the query is: {cmd.CommandText}");
                                            }
                                            object v;
                                            if (reader.IsDBNull(i)) {
                                                v = null;
                                            } else {
                                                if (reader.GetFieldType(i) == typeof(byte[])) {
                                                    long fldLength = reader.GetBytes(i, 0, null, 0, 0);
                                                    byte[] bytes = new byte[fldLength];
                                                    long lread = reader.GetBytes(i, 0, bytes, 0, (int)fldLength);
                                                    // TODO: lread may be more to the point then fldLength ?
                                                    v = (PostedFile)bytes;
                                                } else {
                                                    v = reader.GetValue(i);
                                                }
                                            }
                                            currentResult.Add(fldname, (v is DBNull) ? null : v);

                                        }
                                        // Mark the records unchanged, because they are just picked up from the data store (rdbms in this case).
                                        execContext.DataState.SetUnchanged(currentResult);
                                        results.Add(currentResult);
                                        if (!node.IsList) break;

                                    }
                                }
                                if (metaReport != null)
                                {
                                    metaReport.AddResult(n_rows, reader.FieldCount);
                                }
                            } while (reader.NextResult());
                            if (Action(execContext) != null && Action(execContext).RequireEffect)
                            {
                                if (reader.RecordsAffected == 0)
                                {
                                    reader.Close();
                                    throw new OperationCanceledException("Read operation requires effect, but there was none (rows affected was 0 while expected to be greater). Operation was cancelled. Note that, while supported, usage of RequireEffect in select is error prone.");
                                }
                            }
                            reader.Close();
                            if (metaReport != null)
                            {
                                metaReport.RowsAffected = reader.RecordsAffected;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Action(execContext) == null)
                {
                    throw new Exception($"Missing action: {execContext.Action}, operation: {execContext.Operation} for node: {execContext.CurrentNode.NodeKey} (Module: {execContext.ProcessingContext.InputModel.Module})");
                }
                StringBuilder sbError = new StringBuilder(1000);
                sbError.AppendLine($"Action: {execContext.Action}, operation: {execContext.Operation} for node: {execContext.CurrentNode.NodeKey} (Module: {execContext.ProcessingContext.InputModel.Module}");
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string param in parameters)
                    {
                        sb.AppendLine(param);
                    }
                    sbError.AppendLine($"Read(IDataLoaderReadContext execContext) >> SQL: {sb.ToString()}{Environment.NewLine}{sqlQuery}");
                }
                KraftLogger.LogError(sbError.ToString(), ex, execContext);
                metaReport.SetErrorInfo(ex, sbError.ToString());
                throw;
            }
            return results; // TODO: Decide what behavior we want with empty statements. I for one prefer null result, effectively stopping the operation.
        }

        private void LogExecution(string sqlQuery, IDataLoaderContext execContext, List<string> parameters)
        {
            string env = execContext?.ProcessingContext?.InputModel?.KraftGlobalConfigurationSettings?.EnvironmentSettings?.EnvironmentName;
            if (!string.IsNullOrEmpty(env) && env.Equals("development", StringComparison.OrdinalIgnoreCase))
            {
                string parametersAsString = string.Join(Environment.NewLine, parameters);
                string readOrWriteAction = string.Empty;
                if (execContext is IDataLoaderReadContext)
                {
                    readOrWriteAction = "Read";
                }
                else if (execContext is IDataLoaderWriteContext)
                {
                    readOrWriteAction = "Write";
                }
                StringBuilder sb = new StringBuilder(10000);
                sb.Append($"Operation: {readOrWriteAction} {Environment.NewLine}");
                sb.Append($"Nodeset: {execContext.LoadedNodeSet.NodeSet.Name} {Environment.NewLine}");
                sb.Append($"Nodekey: {execContext.NodeKey} {Environment.NewLine}");
                sb.Append($"Parameters: {Environment.NewLine}{parametersAsString} {Environment.NewLine}");
                sb.Append($"SQLQuery: {sqlQuery} {Environment.NewLine}");
                KraftLogger.LogDebug(sb.ToString(), execContext);
            }
        }

        /// <summary>
        /// Unlike read write is called exactly once per each row and not called at all if the row's state does not require actual writing.
        /// </summary>
        /// <param name="execContext"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        protected override object Write(IDataLoaderWriteContext execContext)
        {
            ADOInfo metaReport = null;
            if (execContext is IActionHelpers helper)
            {
                metaReport = helper.NodeMeta.CreateInfo<ADOInfo>();
            }
            var metaNode = execContext as IActionHelpers;

            //IDataLoaderContext execContext, Configuration configuration) {
            string sqlQuery = null;
            List<string> parameters = new List<string>();
            try
            {
                Node node = execContext.CurrentNode;

                // Statement is already selected for the requested operation (While fetching the Configuration
                if (!string.IsNullOrWhiteSpace(Action(execContext)?.Query))
                {
                    // Check if it is valid
                    if (!(execContext.OwnContextScoped is IADOTransactionScope scopedContext))
                    {
                        throw new NullReferenceException("Scoped synchronization and transaction context is not available.");
                    }
                    // Settings should be passed to the scopedContext in the ExternalServiceImp
                    DbConnection conn = scopedContext.Connection;
                    DbTransaction trans = scopedContext.StartADOTransaction();

                    using (DbCommand cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.Parameters.Clear();
                        sqlQuery = ProcessCommand(cmd, Action(execContext).Query, execContext, out parameters);
                        //TODO need refinement (Robert)
                        //string commandWithParamValues = DbCommandDumper.GetCommandText(cmd);
                        if (metaReport != null) {
                            metaReport.ReportSQL(sqlQuery);
                            metaReport.ReportParameters(cmd.Parameters);
                        }
                        LogExecution(sqlQuery, execContext, parameters);
                        using (DbDataReader reader = cmd.ExecuteReader())
                        {
                            do
                            {
                                int n_rows = 0;
                                while (reader.Read())
                                {
                                    n_rows++;
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        string fname = reader.GetName(i);
                                        if (fname == null) continue;
                                        fname = fname.ToLower().Trim();
                                        // fname = fname.Trim(); // TODO: We have to rethink this - lowercasing seems more inconvenience than a viable protection against human mistakes.
                                        if (fname.Length == 0) throw new Exception("Empty field name in a store context in nodedesfinition: " + node.NodeSet.Name);


                                        object v;
                                        if (reader.IsDBNull(i)) {
                                            v = null;
                                        } else {
                                            if (reader.GetFieldType(i) == typeof(byte[])) {
                                                long fldLength = reader.GetBytes(i, 0, null, 0, 0);
                                                byte[] bytes = new byte[fldLength];
                                                long lread = reader.GetBytes(i, 0, bytes, 0, (int)fldLength);
                                                // TODO: lread may be more to the point then fldLength ?
                                                v = (PostedFile)bytes;
                                            } else {
                                                v = reader.GetValue(i);
                                            }
                                        }
                                        execContext.Row[fname] = (v is DBNull) ? null : v;
                                    }
                                }
                                if (metaReport != null)
                                {
                                    metaReport.AddResult(n_rows, reader.FieldCount);
                                }
                                // This is important, we have been doing this for a single result before, but it is better to assume more than one, so that 
                                // update of the data being written can be done more freely - using more than one select statement after writing. This is
                                // probably rare, but having the opportunity is better than not having it.
                            } while (reader.NextResult());
                            if (Action(execContext) != null && Action(execContext).RequireEffect) {
                                if (reader.RecordsAffected == 0) {
                                    reader.Close();
                                    throw new OperationCanceledException("Write operation requires effect, but there was none (rows affected was 0 while expected to be greater). Operation was cancelled.");
                                }
                            }
                            
                            if (metaReport != null)
                            {
                                metaReport.RowsAffected = reader.RecordsAffected;
                            }
                            if (execContext.Operation != OPERATION_DELETE)
                            {
                                execContext.DataState.SetUnchanged(execContext.Row);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sbError = new StringBuilder(1000);
                sbError.AppendLine($"Action: {execContext.Action}, operation: {execContext.Operation} for node: {execContext.CurrentNode.NodeKey} (Module: {execContext.ProcessingContext.InputModel.Module}");
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string param in parameters)
                    {
                        sb.AppendLine(param);
                    }
                    sbError.AppendLine($"Write(IDataLoaderReadContext execContext) >> SQL: {sb.ToString()}{Environment.NewLine}{sqlQuery}");
                }
                KraftLogger.LogError(sbError.ToString(), ex, execContext);
                metaReport.SetErrorInfo(ex, sbError.ToString());
                throw;
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
        protected virtual void BindParameter(DbCommand cmd, string name, ParameterResolverValue? value)
        {
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
                        if (value.Value.Value is PostedFile pf) {
                            AssignParameterType(param, null); // TODO lets see what will happen
                            param.Value = pf.ToBytes();
                        } else {
                            AssignParameterType(param, GetParameterType(value.Value));
                            param.Value = value.Value.Value;
                        }
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
        protected virtual int? GetParameterType(ParameterResolverValue value)
        {
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
        protected virtual void AssignParameterType(DbParameter param, int? type)
        {
            if (type.HasValue)
            {
                param.DbType = (DbType)type.Value;
            }
            else
            {
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
        protected virtual string ProcessCommand(DbCommand cmd, string sql, IDataLoaderContext execContext, out List<string> parameters)
        {
            var scope = execContext.OwnContextScoped; //GetSynchronizeContextScopedAsync().Result;
            if (scope is ADOSynchronizeContextScopedDefault<XConnection> adoscope) {
                adoscope.ConfigureDbCommand(cmd);
            }
            if (max_recursions <= 0) {
                //var scope = GetSynchronizeContextScopedAsync().Result;
                if (scope != null) {
                    if (scope.CustomSettings != null && scope.CustomSettings.ContainsKey(MAX_RECURSIONS_NAME)) { 
                        string s = scope.CustomSettings[MAX_RECURSIONS_NAME];
                        if (int.TryParse(s, out int mr)) {
                            if (mr > 0) {
                                max_recursions= mr;
                            }
                        }
                    }
                }
                if (max_recursions <= 0) max_recursions = MAX_RECURSIONS_DEFAULT;
            }
            parameters = new List<string>();
            if (string.IsNullOrWhiteSpace(sql)) return null; // We just do nothing in this case.
            //Regex reg = new Regex(@"@([a-zA-Z_][a-zA-Z0-9_\$]*)", RegexOptions.None); // This should be configurable
            Regex regex = new Regex(@"(?:\'(?:(?:\'\')|[^\'])*\')|(?:--[^\n\r]*(?:\n|\r)+)|(?:\/\*(?:(?:.|\r|\n)(?!\*\/))*(?:.|\r|\n)?\*\/)|[@]{2,}|(?:@([a-zA-Z_][a-zA-Z0-9_\$]*))");
            string result = BindParameters(cmd, sql, execContext, parameters, regex);
            cmd.CommandText = result;
            return result;
        }

        private string BindParameters(DbCommand cmd, string sql, IDataLoaderContext execContext, List<string> parameters, Regex reg,int recursions = 0)
        {
            string result = reg.Replace(sql, m =>
            {
                if (m.Groups[1].Success)
                {
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
                                parameters.Add($"Paramname: {paramname} and Paramvalue: {v.Value}");
                                return "@" + paramname;
                            case EResolverValueType.Skip:
                                return "@" + paramname;
                            case EResolverValueType.Nonstorable:
                                return "";
                            case EResolverValueType.Invalid:
                                throw new Exception($"Invalid value received for parameter: {paramname}. This is reported by a resolver, check your expression to find out which one does that and why.");
                            case EResolverValueType.ContentType:
                                if ((max_recursions > 0 && recursions > max_recursions) || (max_recursions == 0 && recursions > MAX_RECURSIONS_DEFAULT)) {
                                    throw new OperationCanceledException($"During parameter ({paramname}) binding of an SQL query the maximum recursions limit ({max_recursions}) was reached ({recursions}).");
                                }
                                // Replace text
                                // We ignore all kinds of type info and we just use ToString();
                                string x = v.Value == null ? "" : v.Value.ToString();
                                parameters.Add($"Paramname: {paramname} and Paramvalue: {v.Value}");
                                x = BindParameters(cmd, x, execContext, parameters, reg, recursions + 1);
                                return x;
                        }
                    }// else skip the rest and leave them unbound. TODO: We will see if this is the correct default behavior.
                }
                return m.Value;
            });
            return result;
        }
        #endregion
    }

    //public class ADOLoader<SyncScope>: ADOBase where SyncScope: IPluginsSynchronizeContextScoped, ITransactionScope, new() {
    //    public async override Task<IPluginsSynchronizeContextScoped> GetSynchronizeContextScopedAsync() {
    //        return await Task.FromResult(new SyncScope());
    //    }
    //}

}
