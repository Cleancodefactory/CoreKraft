using Ccf.Ck.Models.Settings;
using Ccf.Ck.Utilities.DependencyContainer;
using Ccf.Ck.Utilities.MemoryCache;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Ccf.Ck.Models.KraftModule
{
    public class KraftModuleCollection
    {
        private readonly DependencyInjectionContainer _DependencyInjectionContainer;
        private readonly ILogger _Logger;

        private IDictionary<string, KraftModule> _KraftModulesCollection;
        public KraftGlobalConfigurationSettings KraftGlobalConfigurationSettings { get; private set; }

        public KraftModuleCollection(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, DependencyInjectionContainer dependencyInjectionContainer, ILogger logger)
        {
            _KraftModulesCollection = new Dictionary<string, KraftModule>();
            KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            _DependencyInjectionContainer = dependencyInjectionContainer;
            _Logger = logger;
        }

        public virtual KraftModule RegisterModule(string directoryName, string moduleName, KraftDependableModule kraftDependableModule, ICachingService cachingService)
        {
            KraftModule module = new KraftModule(directoryName, 
                moduleName, 
                _DependencyInjectionContainer, 
                this, 
                cachingService, 
                kraftDependableModule, 
                KraftGlobalConfigurationSettings);
            if (module != null && module.Key != null && !_KraftModulesCollection.ContainsKey(module.Key))
            {
                _KraftModulesCollection.Add(module.Key, module);
            }
            return module;
        }

        public KraftModule GetModule(string key)
        {
            string pkey = key?.ToLower();
            if (pkey != null)
            {
                if (_KraftModulesCollection.ContainsKey(pkey))
                {
                    return _KraftModulesCollection[pkey];
                }
            }
            return null;
        }

        public List<KraftModule> GetSortedModules()
        {
            List<KraftModule> sortedModules = new List<KraftModule>();
            foreach (KeyValuePair<string, KraftModule> module in _KraftModulesCollection)
            {
                KraftModule kraftModule = module.Value as KraftModule;
                sortedModules.Add(kraftModule);
            }
            return sortedModules;
        }
    }
}
