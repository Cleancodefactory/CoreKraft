using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using Ccf.Ck.SysPlugins.Utilities.ActionQuery.Attributes;

namespace Ccf.Ck.Tools.ActionQueryDocTool
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load necessary assemblies
            Assembly utilitiesAssembly = Assembly.LoadFrom(@"..\..\..\..\..\SysPlugins\Utilities\bin\Debug\net5.0\Ccf.Ck.SysPlugins.Utilities.dll");
            Assembly webAssembly = Assembly.LoadFrom(@"..\..\..\..\..\SysPlugins\Support\ActionQueryLibs\BasicWeb\bin\Debug\net5.0\Ccf.Ck.SysPlugins.Support.ActionQueryLibs.BasicWeb.dll");
            Assembly fileAssembly = Assembly.LoadFrom(@"..\..\..\..\..\SysPlugins\Support\ActionQueryLibs\Files\bin\Debug\net5.0\Ccf.Ck.SysPlugins.Support.ActionQueryLibs.Files.dll");
            Assembly imagesAssembly = Assembly.LoadFrom(@"..\..\..\..\..\SysPlugins\Support\ActionQueryLibs\Images\bin\Debug\net5.0\Ccf.Ck.SysPlugins.Support.ActionQueryLibs.Images.dll");

            // Get all classes
            Type[] utilitiesTypes = utilitiesAssembly.GetTypes();
            Type[] webTypes = webAssembly.GetTypes();
            Type[] fileTypes= fileAssembly.GetTypes();
            Type[] imageTypes = imagesAssembly.GetTypes();

            // Load specific classes needed
            Type loaderPlugin = utilitiesTypes.Where(x => x.Name.Contains("LoaderPlugin")).FirstOrDefault();
            Type nodePlugin = utilitiesTypes.Where(x => x.Name.Contains("NodePlugin")).FirstOrDefault();
            Type webPlugin = webTypes.Where(x => x.Name.Contains("WebLibrary")).FirstOrDefault();
            Type filesPlugin = fileTypes.Where(x => x.Name.Contains("BasicFiles")).FirstOrDefault();
            Type imagesPlugin = imageTypes.Where(x => x.Name.Contains("BasicImageLib")).FirstOrDefault();

            //Type webPlugin = webTypes.Where(x => x.Name.Contains("NodePlugin")).FirstOrDefault();

            // Filter class methods to only include specific methods
            List<MethodInfo> methodInfo = new List<MethodInfo>();
            methodInfo.AddRange(loaderPlugin.GetMethods().Where(x => x.GetCustomAttributes().Any(y => y.GetType().Name == "DocToolAttribute")).ToArray());
            methodInfo.AddRange(nodePlugin.GetMethods().Where(x => x.GetCustomAttributes().Any(y => y.GetType().Name == "DocToolAttribute")).ToArray());
            methodInfo.AddRange(webPlugin.GetMethods().Where(x => x.GetCustomAttributes().Any(y => y.GetType().Name == "DocToolAttribute")).ToArray());
            methodInfo.AddRange(filesPlugin.GetMethods().Where(x => x.GetCustomAttributes().Any(y => y.GetType().Name == "DocToolAttribute")).ToArray());
            methodInfo.AddRange(imagesPlugin.GetMethods().Where(x => x.GetCustomAttributes().Any(y => y.GetType().Name == "DocToolAttribute")).ToArray());

            int counter = 1;
            List<object> exportMethodsInfo = new List<object>();

            foreach (var method in  methodInfo)
            {
                var attribute = method.GetCustomAttribute<DocToolAttribute>();
                var exportMethodMetaData = new
                {
                    label = attribute.label,
                    kind = "CompletionItemKind.Function",
                    data = counter,
                    detail = "Detail " + attribute.summary,
                    documentation = attribute.docLink
                };

                counter++;
                exportMethodsInfo.Add(exportMethodMetaData);
            }

            var path = @"C:\_Development";

            JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(exportMethodsInfo, options);
            File.WriteAllText(Path.Combine(path, "Definition.json"), jsonString);
        }
    }
}
