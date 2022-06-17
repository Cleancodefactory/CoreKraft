using Ccf.Ck.Utilities.MemoryCache;
using Ccf.Ck.Utilities.DependencyContainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ccf.Ck.Libs.Logging;

namespace Ccf.Ck.Models.Settings.Modules
{
    public class KraftModuleConfigurationSettings
    {
        private readonly DependencyInjectionContainer _DependencyInjectionContainer;
        private readonly ICachingService _CachingService;
        public KraftGlobalConfigurationSettings KraftGlobalConfigurationSettings { get; private set; }

        public void Reset()
        {
            NodeSetSettings = new NodeSetSettings();
        }

        public string ModuleName{ get; private set; }

        public KraftModuleConfigurationSettings(DependencyInjectionContainer dependencyInjectionContainer, ICachingService cachingService, 
            KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _DependencyInjectionContainer = dependencyInjectionContainer;
            _CachingService = cachingService;
            NodeSetSettings = new NodeSetSettings();
            KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
        }

        public void LoadDefinedObjects(string moduleName, string configFile)
        {
            ModuleName = moduleName;
            ConsistencyCheck();
            LoadPropertiesInContainer(NodeSetSettings?.SourceLoaderMapping?.NodesDataIterator?.NodesDataIteratorConf, moduleName, configFile);
            LoadPropertiesInContainer(NodeSetSettings?.SourceLoaderMapping?.NodesDataIterator?.NodesDataLoader, moduleName, configFile);
            LoadPropertiesInContainer(NodeSetSettings?.SourceLoaderMapping?.ViewLoader, moduleName, configFile);
            LoadPropertiesInContainer(NodeSetSettings?.SourceLoaderMapping?.LookupLoader, moduleName, configFile);
            LoadPropertiesInContainer(NodeSetSettings?.SourceLoaderMapping?.ResourceLoader, moduleName, configFile);
            LoadPropertiesInContainer(NodeSetSettings?.SourceLoaderMapping?.CustomPlugin, moduleName, configFile);
        }

        public NodeSetSettings NodeSetSettings { get; private set; }

        private void ConsistencyCheck()
        {
            int requiredCount = 1;

            if (NodeSetSettings?.SourceLoaderMapping?.NodesDataIterator?.NodesDataIteratorConf == null)
            {
                throw new ArgumentException("You need one data nodes iterator configured");
            }
            if (NodeSetSettings.SourceLoaderMapping?.ViewLoader?.Where(dl => dl.Default).Skip(requiredCount).Any() == true)
            {
                throw new ArgumentException("More than one Viewloader is configured with Default == true");
            }
            if (NodeSetSettings.SourceLoaderMapping?.LookupLoader?.Where(dl => dl.Default).Skip(requiredCount).Any() == true)
            {
                throw new ArgumentException("More than one LookupLoader is configured with Default == true");
            }

            if (NodeSetSettings.SourceLoaderMapping?.ResourceLoader?.Where(dl => dl.Default).Skip(requiredCount).Any() == true)
            {
                throw new ArgumentException("More than one ResourceLoader is configured with Default == true");
            }
        }

        private void LoadPropertiesInContainer(List<LoaderProperties> loaderProperties, string moduleName, string configFile)
        {
            if (loaderProperties != null && loaderProperties.Count > 0)
            {
                foreach (LoaderProperties loaderProperty in loaderProperties)
                {
                    Utilities.Generic.Utilities.CheckNullOrEmpty(loaderProperty.ImplementationAsString, true);
                    Utilities.Generic.Utilities.CheckNullOrEmpty(loaderProperty.InterfaceAsString, true);
                    LoadPropertiesInContainer(loaderProperty, moduleName, configFile);
                }
            }
        }

        private void LoadPropertiesInContainer(LoaderProperties loaderProperty, string moduleName, string configFile)
        {
            try
            {
                string cacheKeyImplementation = moduleName + loaderProperty.ImplementationAsString;
                string cacheKeyInterface = moduleName + loaderProperty.InterfaceAsString;
                loaderProperty.ImplementationAsType = _CachingService.Get<Type>(cacheKeyImplementation);
                if (loaderProperty.ImplementationAsType == null)
                {
                    Utilities.Generic.Utilities.LoadAssembly(
                    KraftGlobalConfigurationSettings.GeneralSettings.ModulesRootFolders,
                    "_PluginsReferences",
                    loaderProperty.ImplementationAsString.Split(',')[1].Trim() + ".dll", string.Empty);

                    loaderProperty.ImplementationAsType = Type.GetType(loaderProperty.ImplementationAsString, true);
                    _CachingService.Insert(cacheKeyImplementation, loaderProperty.ImplementationAsType);
                }
                loaderProperty.InterfaceAsType = _CachingService.Get<Type>(cacheKeyInterface);
                if (loaderProperty.InterfaceAsType == null)
                {
                    loaderProperty.InterfaceAsType = Type.GetType(loaderProperty.InterfaceAsString);
                    _CachingService.Insert(cacheKeyInterface, loaderProperty.InterfaceAsType);
                }

                TypeInfo typeInfo = loaderProperty.ImplementationAsType.GetTypeInfo();
                if (typeInfo.ImplementedInterfaces.Contains(loaderProperty.InterfaceAsType))
                {
                    _DependencyInjectionContainer.Add(loaderProperty.ImplementationAsType, loaderProperty.InterfaceAsType, cacheKeyImplementation);
                }
                else
                {
                    throw new Exception($"Type: {loaderProperty.ImplementationAsString} doesn't implement {loaderProperty.InterfaceAsString} in {configFile}");
                }
            }
            catch (Exception ex)
            {
                KraftLogger.LogError($"void LoadPropertiesInContainer: Loaderproperty: {loaderProperty} || moduleName: {moduleName} || configFile: {configFile}", ex);
                throw;
            }            
        }
    }
}
