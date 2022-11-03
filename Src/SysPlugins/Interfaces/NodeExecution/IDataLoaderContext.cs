using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// Node execution context as seen by Data loaders
    /// </summary>
    public interface IDataLoaderContext: IModuleElement, ISupportsPluginServiceManager, IDataStateHelperProvider<IDictionary<string, object>> {
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

        Ccf.Ck.Models.DirectCall.InputModel PrepareCallModel(string module = null,
                                                             string nodeset = null,
                                                             string nodepath = null,
                                                             bool isWriteOperation = false,
                                                             EReadAction readAction = EReadAction.Default) {
            var im = new Ccf.Ck.Models.DirectCall.InputModel(module, nodeset, nodepath, isWriteOperation, readAction);
            var sm = ProcessingContext?.InputModel?.SecurityModel;
            im.SecurityModel = new SecurityModelCopy(sm);
            return im;
        }
        Ccf.Ck.Models.DirectCall.InputModel PrepareCallModelAs(string runas,
                                                             string module = null,
                                                             string nodeset = null,
                                                             string nodepath = null,
                                                             bool isWriteOperation = false,
                                                             EReadAction readAction = EReadAction.Default) {
            var im = new Ccf.Ck.Models.DirectCall.InputModel(module, nodeset, nodepath, isWriteOperation, readAction) { RunAs = runas};
            return im;
        }
    }
}
