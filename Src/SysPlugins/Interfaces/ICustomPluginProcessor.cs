using Ccf.Ck.Models.NodeSet;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface ICustomPluginProcessor
    {
        /// <summary>
        /// Executes the plugins in the customPlugin set one after another, passing each the ctx 
        /// </summary>
        /// <param name="customPlugins"></param>
        /// <param name="ctx"></param>
        /// <returns></returns>
        void Execute(IEnumerable<CustomPlugin> customPlugins, INodePluginContext ctx);
    }
}
