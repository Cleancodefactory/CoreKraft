using System;
using Ccf.Ck.Models.NodeRequest;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Iterators.DataNodes
{
    public class NodeContextualBasket : IContextualBasket
    {
        private NodeExecutionContext.Manager _ExecContext;

        public NodeContextualBasket(NodeExecutionContext.Manager execContext) {
            _ExecContext = execContext;
        }

        public bool HasBasketItem(Type t) {
            if (t == typeof(IProcessingContext))
            {
                return true;
            }
            if (t == typeof(InputModel))
            {
                return true;
            }
            if (t == typeof(KraftGlobalConfigurationSettings))
            {
                return true;
            }
            if (t == typeof(NodeExecutionContext.LoaderPluginContext) ||
                t == typeof(NodeExecutionContext.LoaderPluginReadContext) ||
                t == typeof(NodeExecutionContext.LoaderPluginWriteContext)
             ) {
                return true;
            }
            return false;
        }

        public object PickBasketItem(Type t) {
            if (t == typeof(IProcessingContext))
            {
                return _ExecContext.ProcessingContext;
            }
            if (t == typeof(InputModel))
            {
                return _ExecContext.ProcessingContext.InputModel;
            }
            if (t == typeof(KraftGlobalConfigurationSettings))
            {
                return _ExecContext.ProcessingContext.InputModel.KraftGlobalConfigurationSettings;
            }
            if (t == typeof(NodeExecutionContext.LoaderPluginContext) ||
                t == typeof(NodeExecutionContext.LoaderPluginReadContext) ||
                t == typeof(NodeExecutionContext.LoaderPluginWriteContext) ||
                t == typeof(NodeExecutionContext.LoaderPluginWriteAppendContext) // The append context never reaches here and we are not currently prepared to return it.
             ) {
                return _ExecContext.GetLoaderPluginProxy();
            }
            return null;
        }
    }
}
