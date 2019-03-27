using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.Utilities.DependencyContainer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ccf.Ck.Processing.Execution
{
    internal class Utilities
    {
        internal static T GetPlugin<T>(string key, DependencyInjectionContainer dependencyInjectionContainer, KraftModuleConfigurationSettings kraftConfigurationSettings, ELoaderType pluginType, bool isIterator = false) where T : class
        {
            LoaderProperties loaderProperties = GetLoaderProperties(key, kraftConfigurationSettings, pluginType, isIterator);
            return dependencyInjectionContainer.Get(loaderProperties.InterfaceAsType, kraftConfigurationSettings.ModuleName + loaderProperties.ImplementationAsString) as T;
        }

        public static Dictionary<string, string> GetCustomSettings(string loaderName, ELoaderType loaderType, KraftModuleConfigurationSettings moduleSettings, bool isIterator = false)
        {
            LoaderProperties loaderProperties = GetLoaderProperties(loaderName, moduleSettings, loaderType, isIterator);

            if (loaderProperties != null)
            {
                return loaderProperties.CustomSettings;
            }
            else
            {
                throw new NullReferenceException($"Loader properties for loader:{loaderName} are null (internal Dictionary<string, string> GetCustomSettings(string loaderName, Enumerations.ELoaderType loaderType))");
            }
        }

        private static LoaderProperties GetLoaderProperties(string key, KraftModuleConfigurationSettings kraftConfigurationSettings, ELoaderType pluginType, bool isIterator = false)
        {
            LoaderProperties loaderProperties;
            switch (pluginType)
            {
                case ELoaderType.ViewLoader:
                    {
                        loaderProperties = kraftConfigurationSettings.NodeSetSettings.SourceLoaderMapping.ViewLoader.FirstOrDefault(n => n.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase));
                        break;
                    }
                case ELoaderType.DataLoader:
                    {
                        if (isIterator)
                        {
                            loaderProperties = kraftConfigurationSettings.NodeSetSettings.SourceLoaderMapping.NodesDataIterator.NodesDataIteratorConf;
                        }
                        else
                        {
                            loaderProperties = kraftConfigurationSettings.NodeSetSettings.SourceLoaderMapping.NodesDataIterator.NodesDataLoader.FirstOrDefault(s => s.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase));
                        }
                        break;
                    }
                case ELoaderType.LookupLoader:
                    {
                        loaderProperties = kraftConfigurationSettings.NodeSetSettings.SourceLoaderMapping.LookupLoader.FirstOrDefault(n => n.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase));
                        break;
                    }
                case ELoaderType.ResourceLoader:
                    {
                        loaderProperties = kraftConfigurationSettings.NodeSetSettings.SourceLoaderMapping.ResourceLoader.FirstOrDefault(n => n.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase));
                        break;
                    }
                case ELoaderType.CustomPlugin:
                    {
                        loaderProperties = kraftConfigurationSettings.NodeSetSettings.SourceLoaderMapping.CustomPlugin.FirstOrDefault(n => n.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase));
                        break;
                    }
                default:
                    {
                        throw new Exception("Plugintype is not known in Ccf.Ck.Processing.Execution.GetPlugin");
                    }
            }
            return loaderProperties;
        }
    }
}
