using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Interfaces.NodeExecution
{
    /// <summary>
    /// Node execution context as seen by custom plugins
    /// </summary>
    public interface INodeTaskExecutor
    {
        void Execute(
            LoadedNodeSet loaderContextDefinition,
            IProcessingContext processingContext,
            IPluginAccessor<IDataLoaderPlugin> dataLoaderAccessor,
            IPluginAccessor<INodePlugin> pluginAccessor);

        void ExecuteNodeData(
            LoadedNodeSet loaderContextDefinition,
            IProcessingContext processingContext,
            IPluginAccessor<IDataLoaderPlugin> dataLoaderAccessor,
            IPluginAccessor<INodePlugin> pluginAccessor);

        void ExecuteNodeView(
            LoadedNodeSet loaderContextDefinition,
            IProcessingContext processingContext);
    }
}
