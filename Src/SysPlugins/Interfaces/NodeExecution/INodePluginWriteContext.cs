using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface INodePluginWriteContext: INodePluginContext
    {
        /// <summary>
        /// When writing the custom plugins may update and sometimes add values to the dictionary representing the current row
        /// </summary>
        Dictionary<string, object> Row { get; }
    }
}
