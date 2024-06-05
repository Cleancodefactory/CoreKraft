using System.Collections.Generic;
using Ccf.Ck.SysPlugins.Interfaces;

namespace Ccf.Ck.NodePlugins.OrchestratorHelper
{
    public class OrchestratorHelperContext : IPluginsSynchronizeContextScoped
    {
        public OrchestratorHelperContext()
        {
            this.CustomSettings = new Dictionary<string, string>();
        }
        public Dictionary<string, string> CustomSettings { get; set; }
    }
}
