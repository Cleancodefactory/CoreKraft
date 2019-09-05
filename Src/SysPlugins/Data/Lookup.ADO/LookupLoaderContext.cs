using Ccf.Ck.Models.NodeSet;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using Ccf.Ck.SysPlugins.Interfaces.ContextualBasket;

namespace Ccf.Ck.SysPlugins.Lookups.ADO
{
    internal class LookupLoaderContext
    {
        public IPluginAccessor<IDataLoaderPlugin> ExternalService { get; internal set; }
        public IPluginAccessor<INodePlugin> CustomService { get; internal set; }
        public IPluginServiceManager PluginServiceManager { get; internal set; }
        public LoadedNodeSet LoaderContext { get; internal set; }
        public IProcessingContext ProcessingContext { get; internal set; }
        internal void CheckNulls()
        {
            if (ExternalService == null)
            {
                throw new NullReferenceException(nameof(ExternalService));
            }
            if (PluginServiceManager == null)
            {
                throw new NullReferenceException(nameof(PluginServiceManager));
            }
            if (LoaderContext == null)
            {
                throw new NullReferenceException(nameof(LoaderContext));
            }
            if (ProcessingContext == null)
            {
                throw new NullReferenceException(nameof(ProcessingContext));
            }
        }

        private static LookupLoaderContext _Instance;

        public static LookupLoaderContext Instance
        {
            get { return _Instance ?? (_Instance = new LookupLoaderContext()); }
        }

    }
}
