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
using Microsoft.Data.Sqlite;

namespace Ccf.Ck.SysPlugins.Data.Db.ADO
{
    public class ADOSynchronizeContextScopedSQLite: ADOSynchronizeContextScopedDefault<SqliteConnection>
    {

        
        //private static Regex _DynamicParameterRegEx = new Regex(@"%(?<OnlyParameter>.+?)%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _DynamicParameterRegEx = new Regex(@"%(\w+?)%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        

        internal override DbConnection GetConnection() {
            if (_DbConnection == null) {
                //Create connection
                string moduleRoot = System.IO.Path.Combine(KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolder(ProcessingContext.InputModel.Module), ProcessingContext.InputModel.Module);
                // Support for @moduleroot@ variable replacement for connection strings that refer to file(s)
                string connectionString = (CustomSettings != null && CustomSettings.ContainsKey("ConnectionString"))
                         ? CustomSettings["ConnectionString"].Replace("@moduleroot@", moduleRoot)
                         : null;

                if (string.IsNullOrEmpty(connectionString)) {
                    throw new NullReferenceException("The Connection String must not be null or empty.");
                }

                // MatchCollection matches = _DynamicParameterRegEx.Matches(connectionString);
                connectionString = _DynamicParameterRegEx.Replace(connectionString, m => {
                    string varname = m.Groups[1].Value;
                    var val = LoaderContext.Evaluate(varname);
                    if (val.Value == null || val.ValueType == EResolverValueType.Invalid) {
                        KraftLogger.LogError($"Expected parameter in connection string: {m.Groups[1].Value} was not resolved! Check that parameter's expression. It is recommended to not define it on node basis, but only in a nodeset root!");
                        // DECIDED: Throw! Thoughts: What shall we return on error? This is temporary decision - there should be something better or just excepton.
                        throw new Exception("A variable used in the connection string cannot be resolved or resolves to null. Check if all the %<varname>% entries have paramaters matching varname in the node parameters.");
                        //return m.Value;
                    }
                    if (!string.IsNullOrWhiteSpace(val.Value + "")) {
                        return val.Value.ToString();
                    } else {
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
                _DbConnection = KraftProfiler.Current.ProfiledDbConnection(new SqliteConnection(connectionString));
                //_DbConnection = new SqliteConnection(connectionString);

                //_DbConnection.ConnectionString = connectionString; moved to the construction
            }
            if (_DbConnection.State != ConnectionState.Open) {
                _DbConnection.Open();
            }
            return _DbConnection;
        }
    }
}
