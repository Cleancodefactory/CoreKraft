using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.NodePlugins.Base;
using System;
using System.Collections.Generic;
using System.Text;



namespace Ccf.Ck.NodePlugins.Scripter {

    public class NodeScripterSynchronizeContextScopedImp : NodePluginScopedContextBase, ITransactionScope
    {
        public Dictionary<string, string> CustomSettings { get; set; }

        public void CommitTransaction()
        {
            
        }

        public void RollbackTransaction()
        {
            
        }

        public object StartTransaction()
        {
            return null;
        }
    }
}
