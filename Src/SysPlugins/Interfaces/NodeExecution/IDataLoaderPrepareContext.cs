using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using System.Collections.Generic;

// TODO Deprecate this - we use the IDataLoaderReadContext instead. Although a bit bad-sounding it works for both read and write
namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// Node execution context as seen by Data loaders
    /// </summary>
    public interface IDataLoaderPrepareContext: ISupportsPluginServiceManager, IDataStateHelperProvider<IDictionary<string, object>> {
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
        /// Contains the current operation - select, insert, update, delete, prepare
        /// </summary>
        string Operation { get; }

        /// <summary>
        /// Contains the current action - read/write
        /// </summary>
        string Action { get; }

        /// <summary>
        /// The data going for processing (write), results (read).
        /// On read this will be almost always empty, unless the PreNode plugin created some results (not recommended)
        /// </summary>
        public List<Dictionary<string, object>> Results { get; }


        ParameterResolverValue Evaluate(string expressionName, IList<ParameterResolverValue> oldargs = null);
        void BailOut();
    }
}
