using Ccf.Ck.Utilities.MemoryCache;
using Ccf.Ck.Utilities.DependencyContainer;
using Ccf.Ck.Utilities.Generic.Topologies;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Ccf.Ck.Models.Settings;
using System.Linq;
using System;

namespace Ccf.Ck.Models.KraftModule
{
    public class KraftModuleCollection
    {
        private readonly DependencyInjectionContainer _DependencyInjectionContainer;
        private readonly ILogger _Logger;

        private IDictionary<string, IDependable<KraftModule>> _KraftModulesCollection;
        public KraftGlobalConfigurationSettings KraftGlobalConfigurationSettings { get; private set; }

        public KraftModuleCollection(KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings, DependencyInjectionContainer dependencyInjectionContainer, ILogger logger)
        {
            _KraftModulesCollection = new Dictionary<string, IDependable<KraftModule>>();
            KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            _DependencyInjectionContainer = dependencyInjectionContainer;
            _Logger = logger;
        }

        public virtual KraftModule RegisterModule(string directoryName, ICachingService cachingService)
        {
            KraftModule module = new KraftModule(directoryName, _DependencyInjectionContainer, this, cachingService, KraftGlobalConfigurationSettings, _Logger);
            if (module != null && !_KraftModulesCollection.ContainsKey(module.Key))
            {
                _KraftModulesCollection.Add(module.Key, module);
            }
            return module;
        }

        internal IDependable<KraftModule> GetModuleAsDependable(string key)
        {
            string pkey = ConstructValidKey(key);
            if (_KraftModulesCollection.ContainsKey(pkey))
            {
                return _KraftModulesCollection[pkey];
            }
            return null;
        }

        internal string ConstructValidKey(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                return key.ToLower();
            }
            return null;
        }

        public KraftModule GetModule(string key)
        {
            return GetModuleAsDependable(key) as KraftModule;
        }

        public List<KraftModule> GetSortedModules()
        {
            List<KraftModule> sortedModules = new List<KraftModule>();
            foreach (KeyValuePair<string, IDependable<KraftModule>> module in _KraftModulesCollection)
            {
                KraftModule kraftModule = module.Value as KraftModule;
                sortedModules.Add(kraftModule);
            }
            return sortedModules;
        }

        public void ResolveModuleDependencies()
        {
            foreach (var module in _KraftModulesCollection)
            {
                KraftModule kraftModule = module.Value as KraftModule;
                kraftModule.ConstructDependencies();
            }

            _KraftModulesCollection = _KraftModulesCollection.SortByDependencies();
            int k = 0;
            foreach (var module in _KraftModulesCollection)
            {
                KraftModule kraftModule = module.Value as KraftModule;
                //Check the version
                foreach (KeyValuePair<string, IDependable<KraftModule>> dependency in kraftModule.Dependencies)
                {
                    string requiredVersion = kraftModule.KraftModuleRootConf.Dependencies.First(item => item.Key == dependency.Key).Value;
                    KraftModule actualModule = (_KraftModulesCollection.Values.First(m => m.Key == dependency.Key) as KraftModule);
                    string moduleVersion = actualModule.KraftModuleRootConf.Version;
                    KraftModuleVersion versionChecker = new KraftModuleVersion(moduleVersion, requiredVersion);
                    if (!versionChecker.IsEqualOrHigher())
                    {
                        throw new Exception($"The required version: {requiredVersion} defined in module: {kraftModule.Key} is not complient with actual version: {moduleVersion} of module: {actualModule.Key}");
                    }                    
                }

                kraftModule.DependencyOrderIndex = k;
                k++;
                //call sorting for the modules
                kraftModule.Dependencies = kraftModule.Dependencies.SortByDependencyOrderIndex();
            }
        }
    }
}
