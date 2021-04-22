using Ccf.Ck.Models.KraftModule;
using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.Utilities.Generic.Topologies;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ccf.Ck.Web.Middleware
{
    internal class KraftModulesConstruction
    {
        private KraftModuleCollection _ModulesCollection;
        private IHostApplicationLifetime _ApplicationLifetime;

        public KraftModulesConstruction(KraftModuleCollection modulesCollection, IHostApplicationLifetime applicationLifetime)
        {
            _ModulesCollection = modulesCollection;
            _ApplicationLifetime = applicationLifetime;
        }
        private Dictionary<string, string> Collect(List<string> modulesRootFolders)
        {
            Dictionary<string, string> allReferencedModules = new Dictionary<string, string>();
            foreach (string dir in modulesRootFolders)
            {
                string[] moduleDirectories = Directory.GetDirectories(dir);
                foreach (string subdirectory in moduleDirectories)
                {
                    DirectoryInfo moduleDirectory = new DirectoryInfo(subdirectory);
                    if (moduleDirectory.Exists && KraftModule.IsValidKraftModule(moduleDirectory.FullName))
                    {
                        if (!allReferencedModules.ContainsKey(moduleDirectory.Name.ToLower()))
                        {
                            allReferencedModules.Add(moduleDirectory.Name.ToLower(), moduleDirectory.FullName);
                        }
                    }
                }
            }
            return allReferencedModules;
        }

        internal void Init(string defaultStartModule, List<string> modulesRootFolders)
        {
            Dictionary<string, string> allReferencedModules = Collect(modulesRootFolders);
            Dictionary<string, IDependable<KraftDependableModule>> modulesCollection = LoadModulesAsDependable(allReferencedModules);
            KraftDependableModule startDependableModule = modulesCollection[defaultStartModule.ToLower()] as KraftDependableModule;
            ConstructDependencies(startDependableModule, modulesCollection);
            //ConstructOptionalDependencies(startDependableModule, modulesCollection);
            modulesCollection = OrderModulesByDependencies(modulesCollection);
        }

        private Dictionary<string, IDependable<KraftDependableModule>> LoadModulesAsDependable(Dictionary<string, string> allReferencedModules)
        {
            Dictionary<string, IDependable<KraftDependableModule>> modulesCollection = new Dictionary<string, IDependable<KraftDependableModule>>();
            foreach (KeyValuePair<string, string> module in allReferencedModules)
            {
                KraftDependableModule kraftDependableModule = new KraftDependableModule();
                kraftDependableModule = ReadModuleMetaConfiguration(module.Value, module.Key) as KraftDependableModule;
                modulesCollection.Add(kraftDependableModule.Key, kraftDependableModule);
            }
            foreach (KeyValuePair<string, IDependable<KraftDependableModule>> dependableModule in modulesCollection)
            {
                KraftDependableModule dependableModuleEx = dependableModule.Value as KraftDependableModule;
                foreach (KeyValuePair<string, string> item in dependableModuleEx.KraftModuleRootConf.OptionalDependencies)
                {
                    dependableModuleEx.KraftModuleRootConf.Dependencies.Add(item.Key, item.Value);
                } 
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
                    // TODO TODODODODODODOD
                    //KraftModuleVersion versionChecker = new KraftModuleVersion(moduleVersion, requiredVersion);
                    //if (!versionChecker.IsEqualOrHigher())
                    //{
                    //    throw new Exception($"The required version: {requiredVersion} defined in module: {kraftModule.Key} is not complient with actual version: {moduleVersion} of module: {actualModule.Key}");
                    //}
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

        private IDependable<KraftDependableModule> ReadModuleMetaConfiguration(string modulePath, string key)
        {
            KraftDependableModule kraftDependable = new KraftDependableModule();
            try
            {
                //if (IsValidKraftModule(modulePath))
                //{
                using (StreamReader r = new StreamReader(Path.Combine(modulePath, "Dependency.json")))
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
                    kraftDependable.Key = key;
                    return kraftDependable;
                }
                //}
                //else
                //{
                //    //Obviously this folder doesn't contain configuration file, so we are not handling it anymore
                //    return false;
                //}
            }
            catch (Exception boom)
            {
                throw new Exception($"Reading module's meta configuration file failed for module \"{modulePath}\". {boom.Message}");
            }
        }

        //Dictionary<string, string> allReferencedModules = KraftModulesConstruction.Collect(_KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders);
        //foreach (string dir in _KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders)
        //{
        //string[] moduleDirectories = Directory.GetDirectories(dir);
        //foreach (string subdirectory in moduleDirectories)
        //{
        //DirectoryInfo moduleDirectory = new DirectoryInfo(subdirectory);
        //if (moduleDirectory.Name != null && moduleDirectory.Name.Equals("_PluginsReferences", StringComparison.InvariantCultureIgnoreCase))
        //{
        //    //if (env.IsDevelopment())
        //    //{
        //    //    AttachModulesWatcher(moduleDirectory.FullName, false, applicationLifetime, restart);
        //    //}
        //    continue;
        //}
        //ICachingService cachingService = app.ApplicationServices.GetService<ICachingService>();
        //KraftModule kraftModule = modulesCollection.GetModule(moduleDirectory.Name);
        //if (kraftModule != null)
        //{
        //    continue;
        //}
        //kraftModule = modulesCollection.RegisterModule(dir, moduleDirectory.Name, cachingService);
        //if (kraftModule == null || !kraftModule.IsInitialized)
        //{
        //    _Logger.LogInformation($"Module not created for directory \"{moduleDirectory.Name}\" because of missing configuration files.");
        //    continue;
        //}
        //KraftStaticFiles.RegisterStaticFiles(app, moduleDirectory.FullName, kraftUrlSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlResourceSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlModuleImages);
        //KraftStaticFiles.RegisterStaticFiles(app, moduleDirectory.FullName, kraftUrlSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlResourceSegment, _KraftGlobalConfigurationSettings.GeneralSettings.KraftUrlModulePublic);
        ////moduleKey2Path.Add(kraftModule.Key.ToLower(), dir);
        ////The application will restart when some files changed in the modules directory and subdirectories but only in RELEASE
        ////Check if module is initialized Robert
        //if (kraftModule.IsInitialized)
        //{
        //    string moduleFullPath = Path.Combine(dir, kraftModule.Key);
        //string path2Data = Path.Combine(moduleFullPath, "Data");
        //    if (!HasWritePermissionOnDir(new DirectoryInfo(path2Data), true))
        //    {
        //        throw new SecurityException($"Write access to folder {path2Data} is required!");
        //}
        //path2Data = Path.Combine(moduleFullPath, "Images");
        //if (!HasWritePermissionOnDir(new DirectoryInfo(path2Data), true))
        //{
        //throw new SecurityException($"Write access to folder {path2Data} is required!");
        //}
        //foreach (string validSubFolder in _ValidSubFoldersForWatching)
        //{
        //AttachModulesWatcher(Path.Combine(moduleFullPath, validSubFolder), true, applicationLifetime, restart);
        //}
        //}
        //}
        //}
        ////try to construct all modules
        //modulesCollection.ResolveModuleDependencies();
        ////_KraftGlobalConfigurationSettings.GeneralSettings.ModuleKey2Path = moduleKey2Path;
        //}
    }
}
