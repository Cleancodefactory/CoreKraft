using Ccf.Ck.Models.Settings;
using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.Utilities.DependencyContainer;
using Ccf.Ck.Utilities.Generic.Topologies;
using Ccf.Ck.Utilities.MemoryCache;
using Ccf.Ck.Utilities.Web.BundleTransformations.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using static Ccf.Ck.Utilities.Web.BundleTransformations.Primitives.TemplateKraftBundle;

namespace Ccf.Ck.Models.KraftModule
{
    public class KraftModule
    {
        //some consts, that should be extracted further
        const string CSS_FOLDER_NAME = "Css";
        const string JS_FOLDER_NAME = "Scripts";
        const string TEMPLATES_FOLDER_NAME = "Templates";
        const string DEPENDENCY_FILE_NAME = "Dependency.json";
        const string CONFIGURATION_FILE_NAME = "Configuration.json";
        const string NODESET_NAME = "NodeSets";
        const string RESOURCEDEPENDENCY_FILE_NAME = "Module.dep";

        private readonly DependencyInjectionContainer _DependencyInjectionContainer;
        private readonly KraftGlobalConfigurationSettings _KraftGlobalConfigurationSettings;

        public ScriptKraftBundle ScriptKraftBundle { get; private set; }
        public string ModulePath { get; private set; }
        public TemplateKraftBundle TemplateKraftBundle { get; private set; }
        public StyleKraftBundle StyleKraftBundle { get; private set; }
        public KraftModuleConfigurationSettings ModuleSettings { get; private set; }
        public KraftModuleRootConf KraftModuleRootConf { get; private set; }
        public string Key { get; private set; }
        public string Name { get; private set; }
        //public bool IsInitialized { get; private set; }
        public string DirectoryName { get; private set; }
        public int DependencyOrderIndex { get; private set; }
        public IDictionary<string, KraftModule> Dependencies { get; private set; }

        public Dictionary<string, string> NodeSetMappings { get; set; }

        internal KraftModule(string directoryName, string moduleName,
            DependencyInjectionContainer dependencyInjectionContainer,
            KraftModuleCollection moduleCollection,
            ICachingService cachingService,
            KraftDependableModule kraftDependableModule,
            KraftGlobalConfigurationSettings kraftGlobalConfigurationSettings)
        {
            _DependencyInjectionContainer = dependencyInjectionContainer;
            DirectoryName = directoryName;
            _KraftGlobalConfigurationSettings = kraftGlobalConfigurationSettings;
            Key = kraftDependableModule.Key;
            Name = kraftDependableModule.Name;
            //dependencies
            Dependencies = new Dictionary<string, KraftModule>();
            foreach (KeyValuePair<string, IDependable<KraftDependableModule>> item in kraftDependableModule.Dependencies)
            {
                Dependencies.Add(item.Key, moduleCollection.GetModule(item.Key));
            }

            DependencyOrderIndex = kraftDependableModule.DependencyOrderIndex;
            KraftModuleRootConf = kraftDependableModule.KraftModuleRootConf;

            //Bundles initialize
            StyleKraftBundle = null;
            ScriptKraftBundle = null;
            TemplateKraftBundle = null;

            ModulePath = Path.Combine(DirectoryName, moduleName);

            NodeSetMappings = new Dictionary<string, string>();
            if (Directory.Exists(Path.Combine(ModulePath, NODESET_NAME)))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(ModulePath, NODESET_NAME));
                foreach (DirectoryInfo dirInfo in directoryInfo.GetDirectories())
                {
                    NodeSetMappings.Add(dirInfo.Name.ToLower(), dirInfo.FullName);
                }
            }

            //read configs and dependencies
            InitConfiguredPlugins(Key, Path.Combine(ModulePath, CONFIGURATION_FILE_NAME), cachingService);
        }

        public void ConstructResources(ICachingService cachingService, string rootPath, string startDepFile, bool isScript)
        {
            if (isScript)
            {
                //process Scripts folder
                ScriptKraftBundle = ConstructScriptsBundle(rootPath, Path.Combine(ModulePath, JS_FOLDER_NAME), startDepFile);

                //process Template folder               
                TemplateKraftBundle = ConstructTmplResBundle(rootPath, Path.Combine(ModulePath, TEMPLATES_FOLDER_NAME));
            }
            else
            {
                //process CSS folder
                StyleKraftBundle = ConstructStyleBundle(rootPath, Path.Combine(ModulePath, CSS_FOLDER_NAME), startDepFile);
            }
        }

        public static bool IsValidKraftModule(string modulePath)
        {
            return File.Exists(Path.Combine(modulePath, DEPENDENCY_FILE_NAME));
        }

        private void InitConfiguredPlugins(string moduleKey, string configFile, ICachingService cachingService)
        {
            //Init the kraft module configurations model
            ModuleSettings = new KraftModuleConfigurationSettings(_DependencyInjectionContainer, cachingService, _KraftGlobalConfigurationSettings);

            //read the module configuration
            IConfigurationBuilder configbuilder = new ConfigurationBuilder();
            configbuilder.SetBasePath(Path.GetDirectoryName(configFile)).AddJsonFile(configFile);
            IConfigurationRoot configurationRoot = configbuilder.Build();
            configurationRoot.Bind("KraftModuleConfigurationSettings", ModuleSettings);

            ModuleSettings.LoadDefinedObjects(moduleKey, configFile);
        }

        private StyleKraftBundle ConstructStyleBundle(string rootPath, string resFolderPath, string resProfileFileName)
        {
            string resProfilePhysFileName = Path.Combine(resFolderPath, resProfileFileName);
            if (Directory.Exists(resFolderPath) && File.Exists(resProfilePhysFileName))
            {
                StyleKraftBundle resBundle = new StyleKraftBundle
                {
                    ContentRootPath = rootPath
                };
                //KraftBundle resBundle = new StyleKraftBundle() { ContentRootPath = _KraftEnvironmentSettings.ContentRootPath };
                KraftBundleProfiles resBundleProfile = resBundle as KraftBundleProfiles;
                resBundleProfile.ContentRootPath = rootPath;
                if (resBundleProfile != null)
                {
                    resBundleProfile.StartDirPath = resFolderPath;
                    resBundleProfile.ProfileFiles = new List<string> { resProfileFileName, RESOURCEDEPENDENCY_FILE_NAME };//The default should be last
                    return resBundle;
                }
            }
            return null;
        }

        private ScriptKraftBundle ConstructScriptsBundle(string rootPath, string resFolderPath, string resProfileFileName)
        {
            string resProfilePhysFileName = Path.Combine(resFolderPath, resProfileFileName);
            if (Directory.Exists(resFolderPath) && File.Exists(resProfilePhysFileName))
            {
                ScriptKraftBundle resBundle = new ScriptKraftBundle
                {
                    ContentRootPath = rootPath
                };
                //KraftBundle resBundle = new StyleKraftBundle() { ContentRootPath = _KraftEnvironmentSettings.ContentRootPath };
                KraftBundleProfiles resBundleProfile = resBundle as KraftBundleProfiles;
                resBundleProfile.ContentRootPath = rootPath;
                if (resBundleProfile != null)
                {
                    resBundleProfile.StartDirPath = resFolderPath;
                    resBundleProfile.ProfileFiles = new List<string> { resProfileFileName, RESOURCEDEPENDENCY_FILE_NAME };//The default should be last
                    return resBundle;
                }
            }
            return null;
        }

        private TemplateKraftBundle ConstructTmplResBundle(string rootPath, string tmplFolderPath)
        {
            if (Directory.Exists(tmplFolderPath))
            {
                TemplateKraftBundle resBundle = new TemplateKraftBundle { ContentRootPath = rootPath };
                IFileProvider fileProv = new PhysicalFileProvider(tmplFolderPath);
                IDirectoryContents dirContents = fileProv.GetDirectoryContents("");
                resBundle.StartDirPath = tmplFolderPath;
                resBundle.ModuleName = Key;
                foreach (IFileInfo file in dirContents)
                {
                    string fileExtension = Path.GetExtension(file.PhysicalPath);
                    TemplateFile templateFile = new TemplateFile { TemplateName = file.Name.ToLower().Replace(fileExtension, string.Empty), PhysicalPath = file.PhysicalPath };
                    resBundle.TemplateFiles.Add(templateFile);
                }
                return resBundle;
            }
            return null;
        }
    }
}
