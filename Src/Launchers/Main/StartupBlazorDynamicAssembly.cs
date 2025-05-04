using Ccf.Ck.Launchers.Main.ActionFilters;
using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;

namespace Ccf.Ck.Launchers.Main
{
    internal class StartupBlazorDynamicAssembly
    {
        KraftGlobalConfigurationSettings _KraftGlobalConfiguration;
        Assembly _BlazorAssembly;
        Type _StartupType;

        internal StartupBlazorDynamicAssembly(KraftGlobalConfigurationSettings kraftGlobalConfiguration)
        {
            _KraftGlobalConfiguration = kraftGlobalConfiguration;
        }

        internal void LoadBlazorAssembly()
        {
            foreach (string rootFolder in _KraftGlobalConfiguration.GeneralSettings.ModulesRootFolders)
            {
                string rootPath = Path.Combine(rootFolder, "_PluginsReferences");
                FileInfo assemblyFile;
                foreach (string codeAssembly in _KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.BlazorAssemblyNamesCode)
                {
                    assemblyFile = new FileInfo(Path.Combine(rootPath, codeAssembly));
                    if (assemblyFile.Exists)
                    {
                        _BlazorAssembly = Assembly.LoadFile(assemblyFile.FullName);
                    }
                    else
                    {
                        KraftLogger.LogCritical($"Configured assembly {assemblyFile} is missing or error during compilation! No landing page will be loaded.");
                    }
                }
            }
        }

        internal void ConfigureServices(WebApplicationBuilder builder)
        {
            IMvcBuilder mvcBuilder = builder.Services.AddControllers();

            // Add the dynamically loaded assembly to the ApplicationPartManager
            mvcBuilder
                .PartManager
                .ApplicationParts
                .Add(new AssemblyPart(_BlazorAssembly));

            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(CultureActionFilter));
            });
            // Get the startup type
            string name = _KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.BlazorInitModuleType.NameWithNamespace;
            if (!string.IsNullOrEmpty(name))
            {
                _StartupType = _BlazorAssembly.GetType(name);

                if (_StartupType != null)
                {
                    MethodInfo configureServicesMethod = _StartupType.GetMethod(_KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.BlazorInitModuleType.ConfigureServiceMethodName, BindingFlags.Public | BindingFlags.Static);
                    configureServicesMethod?.Invoke(null, new object[] { builder });
                }
            }            
        }

        internal void Configure(WebApplication app)
        {
            Type appType = _BlazorAssembly.GetType(_KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.BlazorStartApplicationWithNamespace);

            if (_StartupType != null)
            {
                var configureAppMethod = _StartupType.GetMethod(_KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.BlazorInitModuleType.ConfigureAppMethodName, BindingFlags.Public | BindingFlags.Static);
                configureAppMethod?.Invoke(null, new object[] { app });
            }
        }
    }
}
