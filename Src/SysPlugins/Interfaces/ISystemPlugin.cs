using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface ISystemPlugin : IPlugin
    {
        IProcessingContext Execute(LoadedNodeSet loaderContext,
                                            IProcessingContext processingContext,
                                            IPluginServiceManager pluginServiceManager,
                                            IPluginsSynchronizeContextScoped contextScoped,
                                            INode currentNode);
    }
}
