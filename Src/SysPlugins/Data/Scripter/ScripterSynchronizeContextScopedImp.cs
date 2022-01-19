using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.SysPlugins.Data.Scripter
{
    public class ScripterSynchronizeContextScopedImp : IPluginsSynchronizeContextScoped, ITransactionScope, IFileTransactionSupport {
        public Dictionary<string, string> CustomSettings { get; set; }

        private List<string> _deleteOnRollback = new List<string>();
        private List<string> _deleteOnCommit = new List<string>();

        public void DeleteOnRollback(string filepath) {
            if (!string.IsNullOrWhiteSpace(filepath)) {
                _deleteOnRollback.Add(filepath);
                if (_deleteOnCommit.IndexOf(filepath) >= 0) {
                    _deleteOnCommit.Remove(filepath);
                }
            }
        }
        public void DeleteOnCommit(string filepath) {
            if (!string.IsNullOrWhiteSpace(filepath)) {
                _deleteOnCommit.Add(filepath);
                if (_deleteOnRollback.IndexOf(filepath) >= 0) {
                    _deleteOnRollback.Remove(filepath);
                }
            }
        }

        public void CommitTransaction() {
            // Nothing to do
            try {
                for (int i = 0; i < _deleteOnCommit.Count; i++) {
                    var filepath = _deleteOnCommit[i];
                    File.Delete(filepath);
                }
            } finally {
                _deleteOnRollback.Clear();
            }
        }

        public void RollbackTransaction() {
            try {
                for (int i = 0; i < _deleteOnRollback.Count; i++) {
                    var filepath = _deleteOnRollback[i];
                    File.Delete(filepath);
                }
            } finally {
                _deleteOnCommit.Clear();
            }
        }

        public object StartTransaction() {
            return null;
        }
    }
}
