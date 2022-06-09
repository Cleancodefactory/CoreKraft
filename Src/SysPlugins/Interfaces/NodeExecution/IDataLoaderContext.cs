using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// Node execution context as seen by Data loaders
    /// </summary>
    public interface IDataLoaderContext: ISupportsPluginServiceManager, IDataStateHelperProvider<IDictionary<string, object>> {
        IPluginsSynchronizeContextScoped OwnContextScoped { get; }
        IPluginsSynchronizeContextScoped DataLoaderContextScoped { get; }
        Node CurrentNode { get; }
        IPluginAccessor<INodePlugin> CustomService { get; }
        LoadedNodeSet LoadedNodeSet { get; }
        Dictionary<string, object> ParentResult { get; }
        //IPluginServiceManager PluginServiceManager { get; }
        IProcessingContext ProcessingContext { get; }

        string Path { get; }

        string NodeKey { get; }

        /// <summary>
        /// Contains the current operation - select, insert, update, delete
        /// </summary>
        string Operation { get; }

        /// <summary>
        /// Contains the current action - read/write
        /// </summary>
        string Action { get; }
        

        ParameterResolverValue Evaluate(string expressionName, IList<ParameterResolverValue> oldargs = null);
        void BailOut();
    }
}
