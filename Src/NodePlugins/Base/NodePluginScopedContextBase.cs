using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Generic;


namespace Ccf.Ck.NodePlugins.Base
{
    class NodePluginScopedContextBase : IPluginsSynchronizeContextScoped
    {
        public Dictionary<string, string> CustomSettings { get; set; } = new Dictionary<string, string>();
    }
}
