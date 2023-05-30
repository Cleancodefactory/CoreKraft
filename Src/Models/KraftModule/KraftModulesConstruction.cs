using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.Utilities.Generic.Topologies;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ccf.Ck.Models.KraftModule
{
    public class KraftModulesConstruction
    {
        private struct ModuleProps
        {
            public string Name; 
            public string Key;
            public string Path;
        }

        public Dictionary<string, IDependable<KraftDependableModule>> Init(string defaultStartModule, List<string> modulesRootFolders)
        {
            Dictionary<string, ModuleProps> allReferencedModules = Collect(modulesRootFolders);
            Dictionary<string, IDependable<KraftDependableModule>> modulesCollection = LoadModulesAsDependable(allReferencedModules);
            KraftDependableModule startDependableModule = modulesCollection[defaultStartModule.ToLower()] as KraftDependableModule;
            ConstructDependencies(startDependableModule, modulesCollection);
            return OrderModulesByDependencies(modulesCollection);
        }

        private Dictionary<string, ModuleProps> Collect(List<string> modulesRootFolders)
        {
            Dictionary<string, ModuleProps> allReferencedModules = new Dictionary<string, ModuleProps>();
            foreach (string dir in modulesRootFolders)
            {
                DirectoryInfo moduleDirectory = new DirectoryInfo(dir);
                if (KraftModule.IsValidKraftModule(moduleDirectory.FullName))
                {
                    allReferencedModules = CollectModule(moduleDirectory, allReferencedModules);
                }
                else //step inside
                {
                    string[] moduleDirectories = Directory.GetDirectories(dir);
                    foreach (string subdirectory in moduleDirectories)
                    {
                        moduleDirectory = new DirectoryInfo(subdirectory);
                        allReferencedModules = CollectModule(moduleDirectory, allReferencedModules);
                    }
                }                
            }
            return allReferencedModules;
        }

        private Dictionary<string, ModuleProps> CollectModule(DirectoryInfo moduleDirectory, Dictionary<string, ModuleProps> allReferencedModules)
        {
            if (moduleDirectory.Exists && KraftModule.IsValidKraftModule(moduleDirectory.FullName))
            {
                if (!allReferencedModules.ContainsKey(moduleDirectory.Name.ToLower()))
                {
                    ModuleProps moduleProps = new ModuleProps();
                    moduleProps.Key = moduleDirectory.Name.ToLower();
                    moduleProps.Name = moduleDirectory.Name;
                    moduleProps.Path = moduleDirectory.FullName;
                    allReferencedModules.Add(moduleProps.Key, moduleProps);
                }
            }
            return allReferencedModules;
        }

        private Dictionary<string, IDependable<KraftDependableModule>> LoadModulesAsDependable(Dictionary<string, ModuleProps> allReferencedModules)
        {
            Dictionary<string, IDependable<KraftDependableModule>> modulesCollection = new Dictionary<string, IDependable<KraftDependableModule>>();
            foreach (ModuleProps moduleProps in allReferencedModules.Values)
            {
                KraftDependableModule kraftDependableModule = ReadModuleMetaConfiguration(moduleProps) as KraftDependableModule;
                kraftDependableModule.KraftModuleRootPath = Directory.GetParent(moduleProps.Path).FullName;
                foreach (KeyValuePair<string, string> item in kraftDependableModule.KraftModuleRootConf.OptionalDependencies)
                {
                    if (allReferencedModules.ContainsKey(item.Key))//Check if module loaded. If true put it safely in the regular dependencies
                    {
                        kraftDependableModule.KraftModuleRootConf.Dependencies.Add(item.Key, item.Value);
                    }
                }
                modulesCollection.Add(kraftDependableModule.Key, kraftDependableModule);
            }
            return modulesCollection;
        }

        public Dictionary<string, IDependable<KraftDependableModule>> OrderModulesByDependencies(IDictionary<string, IDependable<KraftDependableModule>> modulesCollection)
        {
            modulesCollection = modulesCollection.SortByDependencies();
            int k = 0;
            foreach (KeyValuePair<string, IDependable<KraftDependableModule>> module in modulesCollection)
            {
                KraftDependableModule kraftModule = module.Value as KraftDependableModule;
                //Check the version
                foreach (KeyValuePair<string, IDependable<KraftDependableModule>> dependency in kraftModule.Dependencies)
                {
                    string requiredVersion = kraftModule.KraftModuleRootConf.Dependencies.First(item => item.Key == dependency.Key).Value;
                    KraftDependableModule actualModule = (modulesCollection.Values.First(m => m.Key == dependency.Key) as KraftDependableModule);
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
            return modulesCollection as Dictionary<string, IDependable<KraftDependableModule>>;
        }

        private void ConstructDependencies(KraftDependableModule kraftDependableModule, IDictionary<string, IDependable<KraftDependableModule>> modulesCollection)
        {
            kraftDependableModule.Dependencies = new Dictionary<string, IDependable<KraftDependableModule>>();
            foreach (KeyValuePair<string, string> dependency in kraftDependableModule.KraftModuleRootConf.Dependencies)
            {
                KraftDependableModule depModule = modulesCollection[dependency.Key] as KraftDependableModule;
                if (depModule == null)
                {
                    throw new Exception($"No module with a key \"{dependency.Key}\" is loaded!");
                }
                depModule.Key = dependency.Key;
                kraftDependableModule.Dependencies.Add(depModule.Key, depModule);
                ConstructDependencies(depModule, modulesCollection);
            }
        }

        private IDependable<KraftDependableModule> ReadModuleMetaConfiguration(ModuleProps moduleProps)
        {
            KraftDependableModule kraftDependable = new KraftDependableModule();
            try
            {
                using (StreamReader r = new StreamReader(Path.Combine(moduleProps.Path, "Dependency.json")))
                {
                    Dictionary<string, string> depVersion = new Dictionary<string, string>();
                    kraftDependable.KraftModuleRootConf = JsonConvert.DeserializeObject<KraftModuleRootConf>(r.ReadToEnd());
                    foreach (KeyValuePair<string, string> item in kraftDependable.KraftModuleRootConf.Dependencies)
                    {
                        depVersion.Add(item.Key.ToLower(), item.Value);
                    }
                    kraftDependable.KraftModuleRootConf.Dependencies = depVersion;
                    depVersion = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, string> item in kraftDependable.KraftModuleRootConf.OptionalDependencies ?? new Dictionary<string, string>())
                    {
                        depVersion.Add(item.Key.ToLower(), item.Value);
                    }
                    kraftDependable.KraftModuleRootConf.OptionalDependencies = depVersion;
                    kraftDependable.Key = moduleProps.Key;
                    kraftDependable.Name = moduleProps.Name;
                    return kraftDependable;
                }
            }
            catch (Exception boom)
            {
                throw new Exception($"Reading module's meta configuration file failed for module \"{moduleProps.Name}\". {boom.Message}");
            }
        }
    }
}
