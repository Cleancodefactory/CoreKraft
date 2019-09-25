using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Data.RequestProxy
{
    public class RequestProxySynchronizeContextScopedImp : IPluginsSynchronizeContextScoped
    {
        public Dictionary<string, string> CustomSettings { get; set; }
    }
}
