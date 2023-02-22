using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ccf.Ck.Utilities.Profiling;
using Ccf.Ck.Libs.Web.Bundling;
using Ccf.Ck.Libs.Web.Bundling.Transformations;
using Ccf.Ck.Utilities.Web.BundleTransformations;
using Ccf.Ck.Utilities.MemoryCache;
using Ccf.Ck.Models.Settings;
using Ccf.Ck.Libs.Web.Bundling.Interfaces;

namespace Ccf.Ck.Models.KraftModule
{
    public static class ExtensionMethods
    {
        const string RESOURCEDEPENDENCY_FILE_NAME = "Module.dep";
        private static ILogger _Logger;
        private static ICachingService _CachingService;
        private static KraftModuleCollection _ModulesCollection;

        public static void Init(IApplicationBuilder app, ILogger logger)
        {
            _Logger = logger;
            _CachingService = app.ApplicationServices.GetService<ICachingService>();
            _ModulesCollection = app.ApplicationServices.GetService<KraftModuleCollection>();
        }
        #region Render Scripts and Css
        //Called from the Razor-Views or master page
        public static Scripts KraftScripts(this Profile profile, string moduleDepStartFile = RESOURCEDEPENDENCY_FILE_NAME, string rootVirtualPath = "/Modules")
        {
            if (!profile.HasScriptBundle(profile.Key + "-scripts"))
            {
                ScriptBundle scriptBundle = new ScriptBundle(profile.Key + "-scripts", new PhysicalFileProvider(_ModulesCollection.KraftGlobalConfigurationSettings.EnvironmentSettings.ContentRootPath), null, new List<IBundleTransform>(), false);
                StringBuilder contentTemplates = new StringBuilder(10000);
                bool appendDiv = false;

                //try to get the target module
                KraftModule profileTargetModule = _ModulesCollection.GetModule(profile.Key);
                if (profileTargetModule == null)
                {
                    throw new Exception($"No CoreKraft module found for bundle target \"{profile.Key}\"!");
                }

                HashSet<KraftModule> targetDeps = new HashSet<KraftModule>();
                Dive(profileTargetModule, targetDeps);
                List<KraftModule> targetDepsSorted = targetDeps.OrderBy(x => x.DependencyOrderIndex).ToList<KraftModule>();

                foreach (KraftModule kraftDepModule in targetDepsSorted)
                {
                    kraftDepModule.ConstructResources(_CachingService, kraftDepModule.DirectoryName, moduleDepStartFile, true);
                    if (kraftDepModule.ScriptKraftBundle != null)
                    {
                        using (KraftProfiler.Current.Step($"Time loading {kraftDepModule.Key}: "))
                        {
                            scriptBundle.Include(new KraftRequireTransformation().Process(kraftDepModule.ScriptKraftBundle, kraftDepModule.ModulePath, kraftDepModule.Name, rootVirtualPath, _Logger));
                        }
                    }

                    if (kraftDepModule.TemplateKraftBundle != null && kraftDepModule.TemplateKraftBundle.TemplateFiles.Count > 0)
                    {
                        if (appendDiv)
                        {
                            contentTemplates.Append(",");
                        }
                        else
                        {
                            appendDiv = true;
                        }
                        HtmlTransformation htmlTransformation = new HtmlTransformation();
                        Func<StringBuilder, ILogger, StringBuilder> minifyHtml = htmlTransformation.Process;
                        contentTemplates.Append(new KraftHtml2JsAssocArrayTransformation().Process(kraftDepModule.TemplateKraftBundle, minifyHtml, _Logger));
                    }
                }
                scriptBundle.Transforms.Add(new JsCleanupTransformation());
                scriptBundle.IncludeContent("Registers.addRegister(new TemplateRegister(\"module_templates\")); Registers.getRegister(\"module_templates\").$collection= {" + contentTemplates.Append("}"));

                profile.Add(scriptBundle);
                return profile.Scripts;
            }
            else
            {
                return profile.Scripts;
            }
        }

        //Called from the Razor-Views or master page
        public static Styles KraftStyles(this Profile profile, string moduleDepStartFile = RESOURCEDEPENDENCY_FILE_NAME, string rootVirtualPaht = "/Modules")
        {
            if (!profile.HasStyleBundle(profile.Key + "-css"))
            {
                StyleBundle styleBundle = new StyleBundle(profile.Key + "-css", new PhysicalFileProvider(_ModulesCollection.KraftGlobalConfigurationSettings.EnvironmentSettings.ContentRootPath), null, new List<IBundleTransform>(), false);
                //styleBundle.RemoveTransformationType(typeof(LessTransformation));
                StringBuilder contentTemplates = new StringBuilder(10000);

                //try to get the target module
                KraftModule profileTargetModule = _ModulesCollection.GetModule(profile.Key);
                if (profileTargetModule == null)
                {
                    throw new Exception($"No CoreKraft module found for bundle target \"{profile.Key}\"!");
                }

                HashSet<KraftModule> targetDeps = new HashSet<KraftModule>();
                Dive(profileTargetModule, targetDeps);
                List<KraftModule> targetDepsSorted = targetDeps.OrderBy(x => x.DependencyOrderIndex).ToList<KraftModule>();

                foreach (KraftModule kraftDepModule in targetDepsSorted)
                {
                    kraftDepModule.ConstructResources(_CachingService, kraftDepModule.DirectoryName, moduleDepStartFile, false);
                    if (kraftDepModule.StyleKraftBundle != null)
                    {
                        styleBundle.Include(new KraftRequireTransformation().Process(kraftDepModule.StyleKraftBundle, kraftDepModule.ModulePath, kraftDepModule.Name, rootVirtualPaht, _Logger));
                    }
                }

                profile.Add(styleBundle);
                return profile.Styles;
            }
            else
            {
                return profile.Styles;
            }
        }
        #endregion Render Scripts and Css

        static void Dive(KraftModule kmodule, HashSet<KraftModule> deps)
        {
            foreach (var dep in kmodule.Dependencies)
            {
                Dive(dep.Value as KraftModule, deps);
            }
            deps.Add(kmodule);
        }
    }
}
