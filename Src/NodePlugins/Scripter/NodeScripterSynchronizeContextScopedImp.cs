﻿using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.NodePlugins.Base;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;



namespace Ccf.Ck.NodePlugins.Scripter {

    public class NodeScripterSynchronizeContextScopedImp : NodePluginScopedContextBase, ITransactionScope, IFileTransactionSupport
    {

        private List<string> _deleteOnRollback = new List<string>();
        private List<string> _deleteOnCommit = new List<string>();

        public void DeleteOnRollback(string filepath) {
            if (!string.IsNullOrWhiteSpace(filepath)) _deleteOnRollback.Add(filepath);
        }
        public void DeleteOnCommit(string filepath) {
            if (!string.IsNullOrWhiteSpace(filepath)) _deleteOnCommit.Add(filepath);
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
