using Ccf.Ck.Models.Settings;
using Ccf.Ck.Utilities.GlobalAccessor.Sqlite;
using System;
using System.Collections.Generic;

namespace Ccf.Ck.Utilities.GlobalAccessor
{
    public class SystemPersister
    {
        private readonly KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;
        private readonly SqliteDb _Db;

        public SystemPersister(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings ?? throw new ArgumentNullException(nameof(kraftGlobalConfigurationSettings));
            _Db = new SqliteDb(); // Uses static connection
        }

        public void StoreOperation<T>(T operation) where T : IOperation
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            string key = operation.GetKey();
            _Db.Set(key, operation);
        }

        public IEnumerable<T> GetOperations<T>() where T : IOperation
        {
            return _Db.GetAll<T>();
        }

        public void RemoveOperation<T>(T operation) where T : IOperation
        {
            string key = operation.GetKey();
            _Db.Remove<T>(key);
        }
    }
}
