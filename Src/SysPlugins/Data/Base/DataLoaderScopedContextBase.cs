using System.Collections.Generic;
using Ccf.Ck.SysPlugins.Interfaces;

namespace Ccf.Ck.SysPlugins.Data.Base
{
    /// <summary>
    /// Base class for DataLoader plugins. It is not mandatory to use this class, but may be helpful in simple cases.
    /// More complex loaders usually need much more than this class gives them and implementation from ground up is justified.
    /// </summary>
    public class DataLoaderScopedContextBase : IPluginsSynchronizeContextScoped   {
        public Dictionary<string, string> CustomSettings { get; set; } = new Dictionary<string, string>();
    }
}
