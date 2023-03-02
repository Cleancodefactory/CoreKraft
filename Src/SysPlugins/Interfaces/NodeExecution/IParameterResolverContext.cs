using System.Collections.Generic;
using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// The NodeExecutionContext as seen by resolvers
    /// </summary>
    public interface IParameterResolverContext: IModuleElement, IDataStateHelperProvider<IDictionary<string, object>> {
        IPluginAccessor<INodePlugin> CustomService { get; }
        List<Dictionary<string, object>> Datastack { get; }
        
        LoadedNodeSet LoadedNodeSet { get; }
        /// <summary>
        /// Points to the configuration describing the current node.
        /// </summary>
        Node CurrentNode { get; }
        Stack<string> OverrideAction { get; }
        IPluginServiceManager PluginServiceManager { get; }
        IProcessingContext ProcessingContext { get; }
        //string Phase { get; }
        Dictionary<string, object> Row { get; }
        List<Dictionary<string, object>> Results { get; }

        string Path { get; }

        /// <summary>
        /// Contains the current action - read/write
        /// </summary>
        string Action { get; }
        /// <summary>
        /// Contains the current operation - select, insert, update, delete
        /// </summary>
        string Operation { get; }

        string NodeKey { get; }

        bool ParentAccessNotAllowed { get { return false; } }

        IParameterResolverSetManager ResolversManager { get; }

        ParameterResolverValue Evaluate(string expressionName, IList<ParameterResolverValue> oldargs = null);
    }
}