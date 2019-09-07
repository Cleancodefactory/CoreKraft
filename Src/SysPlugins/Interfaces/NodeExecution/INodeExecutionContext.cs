using System.Collections.Generic;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// We can live without this one, it is used only for the class that is never seen by anything else but the iterator.
    /// Still, we keep it anyway
    /// </summary>
    public interface INodeExecutionContext {
        IPluginsSynchronizeContextScoped OwnContextScoped { get; }
        Node CurrentNode { get; }
        IPluginAccessor<INodePlugin> CustomService { get; }
        LoadedNodeSet LoadedNodeSet { get; }
        Dictionary<string, object> ParentResult { get; }
        IPluginServiceManager PluginServiceManager { get; }
        IProcessingContext ProcessingContext { get; }
        ParameterResolverValue Evaluate(string expressionName, IList<ParameterResolverValue> oldargs = null);
    }
}