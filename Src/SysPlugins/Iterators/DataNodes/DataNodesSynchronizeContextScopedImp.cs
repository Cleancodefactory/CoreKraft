using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Iterators.DataNodes
{
    public class DataNodesSynchronizeContextScopedImp : IPluginsSynchronizeContextScoped
    {
        public Dictionary<string, string> CustomSettings { get; set; }
    }
}
