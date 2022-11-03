using System;
using System.Collections.Generic;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces;

namespace Ccf.Ck.SysPlugins.Iterators.DataNodes.CustomPluginsExecution
{
    public class CustomPluginProcessor : ICustomPluginProcessor
    {
        public void Execute(IEnumerable<CustomPlugin> customPlugins, INodePluginContext ctx, Func<bool> bailOut)
        {
            foreach (CustomPlugin customPlugin in customPlugins)
            {
                // localResult = null;
                if (customPlugin != null)
                {
                    INodePlugin plugin = ctx.CustomPluginAccessor.LoadPlugin(customPlugin.CustomPluginName);
                    // Here the customplugin's context is attached
                    // the dataloader's context is attached in the iterator
                    ctx.CustomPlugin = customPlugin;
                    ctx.OwnContextScoped = ctx.CustomPluginAccessor.GetPluginsSynchronizeContextScoped(customPlugin.CustomPluginName, plugin).Result;
                    ctx.OwnContextScoped.SetModuleName(ctx.Module);
                    plugin?.Execute(ctx);
                    if (bailOut()) return;
                }
            }
        }
    }
}
