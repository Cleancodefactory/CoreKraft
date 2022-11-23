using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Iterators.DataNodes;
using Ccf.Ck.Utilities.Profiling;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Data.Db.ADO
{
    public class ADOSynchronizeContextScopedDefault<XConnection> : IPluginsSynchronizeContextScoped, IADOTransactionScope, IContextualBasketConsumer
        where XConnection : DbConnection, new()
    {

        protected DbConnection _DbConnection;
        protected DbTransaction _DbTransaction;
        //private static Regex _DynamicParameterRegEx = new Regex(@"%(?<OnlyParameter>.+?)%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _DynamicParameterRegEx = new Regex(@"%(\w+?)%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private bool _IsParsed;
        private bool _IsStartReadTransaction;

        public KraftGlobalConfigurationSettings KraftGlobalConfigurationSettings => ProcessingContext.InputModel.KraftGlobalConfigurationSettings;
        public IProcessingContext ProcessingContext { get; set; }
        public NodeExecutionContext.LoaderPluginContext LoaderContext { get; set; }



        #region IPluginsSynchronizeContextScoped
        public Dictionary<string, string> CustomSettings
        {
            get; set;
        }
        #endregion

        #region ITransactionScope
        public void CommitTransaction()
        {
            if (_DbTransaction != null)
            {
                _DbTransaction.Commit();
                _DbTransaction.Dispose();
                _DbTransaction = null;
            }
            if (_DbConnection != null)
            {
                _DbConnection.Close();
                _DbConnection.Dispose();
                _DbConnection = null;
                _DbTransaction = null;
            }
        }

        public void RollbackTransaction()
        {
            // We expect that the connection will dispose the transaction

            if (_DbTransaction != null)
            {
                _DbTransaction.Rollback();
                _DbTransaction.Dispose();
                _DbTransaction = null;
            }
            if (_DbConnection != null)
            {
                _DbConnection.Close();
                _DbConnection.Dispose();
                _DbConnection = null;
                _DbTransaction = null;
            }
        }

        public object StartTransaction()
        {
            if (_DbTransaction == null)
            {

                _DbTransaction = GetConnection().BeginTransaction();
            }
            return _DbTransaction;
        }
        public DbTransaction StartADOTransaction()
        {
            return StartTransaction() as DbTransaction;
        }
        #endregion

        #region IADOTransactionScope
        public virtual DbConnection Connection => GetConnection();

        public virtual DbTransaction CurrentTransaction => _DbTransaction;
        #endregion

        internal virtual DbConnection GetConnection()
        {
            if (_DbConnection == null)
            {
                //Create connection
                string moduleRoot = System.IO.Path.Combine(KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolder(ProcessingContext.InputModel.Module), ProcessingContext.InputModel.Module);
                // Support for @moduleroot@ variable replacement for connection strings that refer to file(s)
                string connectionString = (CustomSettings != null && CustomSettings.ContainsKey("ConnectionString"))
                         ? CustomSettings["ConnectionString"].Replace("@moduleroot@", moduleRoot)
                         : null;

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new NullReferenceException("The Connection String must not be null or empty.");
                }

                // MatchCollection matches = _DynamicParameterRegEx.Matches(connectionString);
                connectionString = _DynamicParameterRegEx.Replace(connectionString, m =>
                {
                    string varname = m.Groups[1].Value;
                    if (LoaderContext == null)
                    {
                        throw new Exception("The Loader context is not available at this time.");
                    }
                    var val = LoaderContext.Evaluate(varname);
                    if (val.Value == null || val.ValueType == EResolverValueType.Invalid)
                    {
                        KraftLogger.LogError($"Expected parameter in connection string: {m.Groups[1].Value} was not resolved! Check that parameter's expression. It is recommended to not define it on node basis, but only in a nodeset root!");
                        // DECIDED: Throw! Thoughts: What shall we return on error? This is temporary decision - there should be something better or just excepton.
                        throw new Exception("A variable used in the connection string cannot be resolved or resolves to null. Check if all the %<varname>% entries have paramaters matching varname in the node parameters.");
                        //return m.Value;
                    }
                    if (!string.IsNullOrWhiteSpace(val.Value + ""))
                    {
                        return val.Value.ToString();
                    }
                    else
                    {
                        KraftLogger.LogError($"Expected parameter in connection string: {m.Groups[1].Value} was not found or cannot be resolved!");
                        throw new Exception("A variable used in the connection string cannot be resolved or is null/empty. Check if all the %<varname>% entries have paramaters matching varname in the node parameters.");
                        //return m.Value;
                    }
                });

                /*if (matches.Count > 0) {
                    for (int i = 0; i < matches.Count; i++) {

                        string parameter = matches[i].Groups["OnlyParameter"].ToString();

                        if (ProcessingContext.InputModel.Data.ContainsKey(parameter)) {

                            connectionString = connectionString.Replace(matches[i].ToString(), ProcessingContext.InputModel.Data[parameter].ToString());
                        }
                        else if (ProcessingContext.InputModel.Client.ContainsKey(parameter)) {

                            connectionString = connectionString.Replace(matches[i].ToString(), ProcessingContext.InputModel.Client[parameter].ToString());
                        }
                        else {

                            KraftLogger.LogError($"Expected parameter in connection string: {matches[i]} was not found and the connection string remains invalid! Please consider the casing!");
                            //connectionString = connectionString.Replace(matches[i].ToString(), string.Empty);
                        }
                    }
                }*/

                // TODO Here it times out - before the connection string
                _DbConnection = KraftProfiler.Current.ProfiledDbConnection(new XConnection());

                _DbConnection.ConnectionString = connectionString;
            }
            if (_DbConnection.State != ConnectionState.Open)
            {
                _DbConnection.Open();
            }
            return _DbConnection;
        }

        public void ConfigureDbCommand(DbCommand cmd) {
            if (CustomSettings != null) {
                string commandTimeout = null;
                if (CustomSettings.ContainsKey("commandtimeout"))
                {
                    commandTimeout = CustomSettings["commandtimeout"];
                }
                if (CustomSettings.ContainsKey("CommandTimeout"))
                {
                    commandTimeout = CustomSettings["CommandTimeout"];
                }
                if (int.TryParse(commandTimeout, out int seconds))
                {
                    if (seconds > 0)
                    {
                        cmd.CommandTimeout = seconds;
                    }                    
                }
            }
        }
        #region IContextualBasketConsumer
        public void InspectBasket(IContextualBasket basket)
        {
            var pc = basket.PickBasketItem<IProcessingContext>();
            ProcessingContext = pc;
            LoaderContext = basket.PickBasketItem<NodeExecutionContext.LoaderPluginContext>();
        }
        #endregion IContextualBasketConsumer

        public bool IsStartReadTransaction()
        {
            if (!_IsParsed)
            {
                if (CustomSettings != null && CustomSettings.ContainsKey("StartReadTransaction"))
                {
                    bool.TryParse(CustomSettings["StartReadTransaction"].ToString(), out _IsStartReadTransaction);
                }
                _IsParsed = true;
            }
            return _IsStartReadTransaction;
        }
    }
}
