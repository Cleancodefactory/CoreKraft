using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    public interface IDataIteratorPlugin : IPlugin
    {
        IProcessingContext Execute(LoadedNodeSet loaderContext,
                                            IProcessingContext processingContext,
                                            IPluginServiceManager pluginServiceManager,
                                            IPluginAccessor<IDataLoaderPlugin> dataLoaderAccessor,
                                            IPluginAccessor<INodePlugin> customPluginAccessor = null);
    }
}
