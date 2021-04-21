using Ccf.Ck.Models.KraftModule;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ccf.Ck.Web.Middleware
{
    internal class KraftModulesConstruction
    {
        public KraftModulesConstruction(KraftModuleCollection modulesCollection, IHostApplicationLifetime applicationLifetime)
        {

        }
        internal Dictionary<string, string> Collect(List<string> modulesRootFolders)
        {
            Dictionary<string, string> allReferencedModules = new Dictionary<string, string>();
            foreach (string dir in modulesRootFolders)
            {
                string[] moduleDirectories = Directory.GetDirectories(dir);
                foreach (string subdirectory in moduleDirectories)
                {
                    DirectoryInfo moduleDirectory = new DirectoryInfo(subdirectory);
                    if (moduleDirectory.Name != null && moduleDirectory.Name.Equals("_PluginsReferences", StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                    if (moduleDirectory.Exists && isValidKraftModule)
                    {
                        if (!allReferencedModules.ContainsKey(moduleDirectory.Name))
                        {
                            allReferencedModules.Add(moduleDirectory.Name, moduleDirectory.FullName);
                        }
                    }
                }
            }
            return allReferencedModules;
        }

        internal void Init(string defaultStartModule, List<string> modulesRootFolders)
        {
            Dictionary<string, string> allReferencedModules = Collect(modulesRootFolders);
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
