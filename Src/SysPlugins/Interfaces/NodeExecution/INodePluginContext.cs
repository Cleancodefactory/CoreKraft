using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.Models.Resolvers;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces
{
    /// <summary>
    /// Node execution context as seen by custom plugins
    /// </summary>
    public interface INodePluginContext: ISupportsPluginServiceManager, IDataStateHelperProvider<IDictionary<string, object>> {
        IProcessingContext ProcessingContext { get;}
        IPluginsSynchronizeContextScoped OwnContextScoped { get; set; }
        IPluginsSynchronizeContextScoped DataLoaderContextScoped { get; }
        // IPluginServiceManager PluginServiceManager { get;  }
	    IPluginAccessor<INodePlugin> CustomPluginAccessor { get; }
        Node CurrentNode { get; } // determine what part of this will be needed !!!

        object Data { get; } // The data root (comes from the client or is in a process of generation)

        // Accessible only on Write
        // Dictionary<string, object> Row { get; }

        // Accessible only on Read
        // List<Dictionary<string, object>> Results { get; set; }

        List<Dictionary<string, object>> Datastack { get; }
        string Path { get; }
        //string Phase { get; }
        // public string RowState { get; set; } // Used only when Action = store to indicate the state of the this.Row (this is a helper you have Api.Set/GetRowState as alternative ways to deal with this)
        
        string NodeKey { get; }
        /// <summary>
        /// Contains the current action - read/write
        /// </summary>
        string Action { get; }
        /// <summary>
        /// Contains the current operation - select, insert, update, delete
        /// </summary>
        string Operation { get; }

        NodePluginPhase ExecutionPhase { get; }

        ParameterResolverValue Evaluate(string expressionName, IList<ParameterResolverValue> oldargs = null);

        void BailOut();

        CustomPlugin CustomPlugin { get; set; }
    }
}
