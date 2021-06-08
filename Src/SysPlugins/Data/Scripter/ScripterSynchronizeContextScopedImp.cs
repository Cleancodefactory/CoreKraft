using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.SysPlugins.Data.Scripter
{
    public class ScripterSynchronizeContextScopedImp : IPluginsSynchronizeContextScoped, ITransactionScope
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
