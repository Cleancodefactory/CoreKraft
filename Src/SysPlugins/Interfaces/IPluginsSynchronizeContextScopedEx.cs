using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IPluginsSynchronizeContextScopedEx: IPluginsSynchronizeContextScoped
    {
        /// <summary>
        /// Enables the system to set the plugin definition name in the scope.
        /// Should be implemented if this knowledge is needed by the context
        /// </summary>
        string PluginName { get; set; }
        string ModuleName { get; set; }
    }
}
