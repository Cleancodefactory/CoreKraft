using System;
using System.Collections.Generic;
using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces;
using Ccf.Ck.Utilities.Generic;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Iterators.DataNodes
{
    internal class DataIteratorContext
    {
        public DataIteratorContext()
        {
            Datastack = new ListStack<Dictionary<string, object>>();
            OverrideAction = new Stack<string>();
        }
        public IPluginAccessor<IDataLoaderPlugin> DataLoaderPluginAccessor { get; internal set; }
        public IPluginAccessor<INodePlugin> CustomPluginAccessor { get; internal set; }
        public IPluginServiceManager PluginServiceManager { get; internal set; }
        public LoadedNodeSet LoadedNodeSet { get; internal set; }
        public IProcessingContext ProcessingContext { get; internal set; }
        public ListStack<Dictionary<string, object>> Datastack { get; private set; }
        public Stack<string> OverrideAction { get; private set; }
        /// <summary>
        /// If set the reqursion should stop and bail immediately
        /// </summary>
        public bool BailOut { get; set; } = false;
        internal void CheckNulls()
        {
            if (DataLoaderPluginAccessor == null)
            {
                throw new NullReferenceException(nameof(DataLoaderPluginAccessor));
            }
            if (PluginServiceManager == null)
            {
                throw new NullReferenceException(nameof(PluginServiceManager));
            }
            if (LoadedNodeSet == null)
            {
                throw new NullReferenceException(nameof(LoadedNodeSet));
            }
            if (ProcessingContext == null)
            {
                throw new NullReferenceException(nameof(ProcessingContext));
            }
        }
    }

}
