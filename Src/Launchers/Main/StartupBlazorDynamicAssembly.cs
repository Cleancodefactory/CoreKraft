using Ccf.Ck.Launchers.Main.ActionFilters;
using Ccf.Ck.Libs.Logging;
using Ccf.Ck.Models.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;

namespace Ccf.Ck.Launchers.Main
{
    public class StartupBlazorDynamicAssembly
    {
        KraftGlobalConfigurationSettings _KraftGlobalConfiguration;
        Assembly _BlazorAssembly;
        Type _StartupType;

        public StartupBlazorDynamicAssembly(KraftGlobalConfigurationSettings kraftGlobalConfiguration)
        {
            _KraftGlobalConfiguration = kraftGlobalConfiguration;
        }

        public void LoadBlazorAssembly()
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

        public void ConfigureServices(IServiceCollection services)
        {
            var mvcBuilder = services.AddControllers();

            // Add the dynamically loaded assembly to the ApplicationPartManager
            mvcBuilder
                .PartManager
                .ApplicationParts
                .Add(new AssemblyPart(_BlazorAssembly));
           
            services.AddControllersWithViews(options =>
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
                    var configureServicesMethod = _StartupType.GetMethod(_KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.BlazorInitModuleType.ConfigureServiceMethodName, BindingFlags.Public | BindingFlags.Static);
                    configureServicesMethod?.Invoke(null, new object[] { services });
                }
            }            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime, IEndpointRouteBuilder endpoints)
        {
            Type appType = _BlazorAssembly.GetType(_KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.BlazorStartApplicationWithNamespace);

            endpoints.MapStaticAssets();

            if (_StartupType != null)
            {
                var configureAppMethod = _StartupType.GetMethod(_KraftGlobalConfiguration.GeneralSettings.BlazorAreaAssembly.BlazorInitModuleType.ConfigureAppMethodName, BindingFlags.Public | BindingFlags.Static);
                configureAppMethod?.Invoke(null, new object[] { app });
            }

            MethodInfo method = typeof(RazorComponentsEndpointRouteBuilderExtensions)
                .GetMethod("MapRazorComponents", BindingFlags.Static | BindingFlags.Public);
            if (method != null)
            {
                // Create a generic method using your loaded type
                var genericMethod = method.MakeGenericMethod(appType);
                // Invoke the generic method. The first parameter is null because it's a static method,
                // and the second parameter is an array containing the arguments (here, 'app')
                object cvbuilder = genericMethod.Invoke(null, new object[] { endpoints });
                RazorComponentsEndpointConventionBuilder convention_builder = cvbuilder as RazorComponentsEndpointConventionBuilder;
                if (convention_builder != null)
                {
                    convention_builder.AddInteractiveServerRenderMode();
                }
                else
                {
                    throw new Exception("Convention builder not returned");
                }
            }
            else
            {
                throw new Exception("Cannot find MapRazorComponents method");
            }
        }
    }
}
