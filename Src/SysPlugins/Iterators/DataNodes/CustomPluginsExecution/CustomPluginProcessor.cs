using System.Collections.Generic;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.ContextBasket;

namespace Ccf.Ck.SysPlugins.Iterators.DataNodes
{
    public class CustomPluginProcessor
    {
        public void Execute(IEnumerable<CustomPlugin> customPlugins, NodeExecutionContext.Manager p)
        {
            Dictionary<string,object> oldrow = p.Row;
            foreach (CustomPlugin customPlugin in customPlugins)
            {
                // localResult = null;
                if (customPlugin != null)
                {
                    INodePlugin plugin = p.CustomService.LoadPlugin(customPlugin.CustomPluginName);
                    // Here the customplugin's context is attached
                    // the dataloader's context is attached in the iterator
                    p.OwnContextScoped = p.CustomService.GetPluginsSynchronizeContextScoped(customPlugin.CustomPluginName, plugin).Result;
                    if (p.Action == ModelConstants.ACTION_READ) {
                        plugin?.Execute(p.CustomPluginProxy);
                    } else {
                        if (p.Results != null) {
                            foreach (Dictionary<string, object> row in p.Results) {
                                p.Row = row;
                                plugin?.Execute(p.CustomPluginProxy); // CustomPluginProxy returns pre-set appropriate proxy for read and write.   
                            }
                            p.Row = oldrow;
                        } else {
                            plugin?.Execute(p.CustomPluginProxy);
                        }
                    }
                    
                }
            }
        }
    }
}
