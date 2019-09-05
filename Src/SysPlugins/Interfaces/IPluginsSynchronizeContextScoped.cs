using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IPluginsSynchronizeContextScoped
    {
        Dictionary<string, string> CustomSettings { get; set; }
    }
}
