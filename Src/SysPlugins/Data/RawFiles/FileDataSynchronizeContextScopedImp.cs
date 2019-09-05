using Ccf.Ck.SysPlugins.Data.FileTransaction;
using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Data.RawFiles
{
    public class FileDataSynchronizeContextScopedImp : IPluginsSynchronizeContextScoped, ITransactionScope
    {
        private bool _FirstCall;
        private FileSystemTransaction _FileSystemTransaction;
        public FileDataSynchronizeContextScopedImp()
        {
            _FirstCall = true;
        }
        public Dictionary<string, string> CustomSettings { get; set; }
        public string OriginalFileName { get; set; }

        public void CommitTransaction()
        {
            if (_FileSystemTransaction != null)
            {
                _FileSystemTransaction.Commit();
            }
            //TODO create temp file, if success del the old and rename the new one
        }

        public void RollbackTransaction()
        {
            //TODO delete the new if fail
            if (_FileSystemTransaction != null)
            {
                _FileSystemTransaction.Rollback();
            }            
        }

        public object StartTransaction()
        {
            if (_FirstCall)
            {
                _FirstCall = false;
                _FileSystemTransaction = new FileSystemTransaction();
            }
            return _FileSystemTransaction;
        }

    }
}
