using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Lookups.ADO
{
    public class LookupADOSynchronizeContextScopedDefault<XConnection> : IPluginsSynchronizeContextScoped, IADOTransactionScope
        where XConnection : DbConnection, new()
    {

        private DbConnection _DbConnection;
        private DbTransaction _DbTransaction;

        public KraftGlobalConfigurationSettings KraftGlobalConfigurationSettings => ProcessingContext.InputModel.KraftGlobalConfigurationSettings;
        public IProcessingContext ProcessingContext => LookupLoaderContext.Instance.ProcessingContext;

        #region IPluginsSynchronizeContextScoped
        public Dictionary<string, string> CustomSettings { get; set; }
        #endregion

        #region ITransactionScope
        public virtual DbConnection Connection => GetConnection();

        public virtual DbTransaction CurrentTransaction => _DbTransaction;

        public void CommitTransaction()
        {
            //throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            //throw new NotImplementedException();
        }

        public DbTransaction StartADOTransaction()
        {
            if (_DbTransaction == null)            {

                _DbTransaction = GetConnection().BeginTransaction();
            }
            return _DbTransaction;
        }

        public object StartTransaction()
        {
            //throw new NotImplementedException();
            return null;
        }

        #endregion

        internal DbConnection GetConnection()
        {
            if (_DbConnection == null)
            {
                //Create connection
                string moduleRoot = System.IO.Path.Combine(KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolder(ProcessingContext.InputModel.Module), ProcessingContext.InputModel.Module);
                // Support for @moduleroot@ variable replacement for connection strings that refer to file(s)
                string ConnectionString = (CustomSettings != null && CustomSettings.ContainsKey("ConnectionString"))
                         ? CustomSettings["ConnectionString"].Replace("@moduleroot@", moduleRoot)
                         : null;

                if (string.IsNullOrEmpty(ConnectionString))
                {
                    throw new NullReferenceException("The Connection String must not be null or empty.");
                }
                _DbConnection = new XConnection();
                _DbConnection.ConnectionString = ConnectionString;
            }
            if (_DbConnection.State != ConnectionState.Open)
            {
                _DbConnection.Open();
            }
            return _DbConnection;
        }
    }
}
