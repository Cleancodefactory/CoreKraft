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
            if (args.Length == 1)
            {
                try
                {
                    List<Assembly> loadedAssemblies = LoadAssemblies();
                    List<object> exportMethodsInfo = CollectCustomAttributes(loadedAssemblies);
                    string path = args[0];
                    JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(exportMethodsInfo, options);
                    File.WriteAllText(path, jsonString);
                    Console.WriteLine("Success extracting meta info for the exported functions");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("Full path to export file as parameter needed");
                Console.ReadLine();
            }            
        }

        private static List<Assembly> LoadAssemblies()
        {
            //Load all available assemblies in the bin folder and make them available for CustomAttribute search
            List<Assembly> loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            string[] loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

            string[] referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            List<string> toLoad = referencedPaths.Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase)).ToList();

            toLoad.ForEach(path => loadedAssemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path))));
            //loadedAssemblies contains all the possible referenced types loaded
            return loadedAssemblies;
        }

        private static List<object> CollectCustomAttributes(List<Assembly> loadedAssemblies)
        {
            List<MethodAttributes> methodAttributesCollection = new List<MethodAttributes>();

            foreach (Assembly assembly in loadedAssemblies)
            {
                Type[] types = assembly.GetTypes();
                foreach (Type classType in types)
                {
                    foreach (MethodInfo methodInfo in classType.GetMethods())
                    {
                        Attribute functionAttribute = methodInfo.GetCustomAttribute(typeof(FunctionAttribute), false);
                        if (functionAttribute != null)//only when FunctionAttribute present
                        {
                            MethodAttributes methodAttributes = new MethodAttributes();
                            methodAttributes.FunctionAttribute = (FunctionAttribute)functionAttribute;
                            methodAttributes.ParameterAttributes = methodInfo.GetCustomAttributes(typeof(ParameterAttribute)).Cast<ParameterAttribute>().ToList();
                            methodAttributes.ParameterPatternAttributes = methodInfo.GetCustomAttributes(typeof(ParameterPatternAttribute)).Cast<ParameterPatternAttribute>().ToList();
                            methodAttributes.ResultAttribute = (ResultAttribute)methodInfo.GetCustomAttribute(typeof(ResultAttribute));
                            methodAttributesCollection.Add(methodAttributes);
                        }
                    }
                }
            }

            int counter = 1;
            List<object> exportMethodsInfo = new List<object>();

            foreach (MethodAttributes methodAttributes in methodAttributesCollection)
            {
                var exportMethodMetaData = new
                {
                    label = methodAttributes.FunctionAttribute.FunctionName,
                    kind = 3,//CompletionItemKind.Function
                    data = counter,
                    detail = methodAttributes.CompileDetail(),
                    documentation = methodAttributes.CompileDocumentation()
                };

                counter++;
                exportMethodsInfo.Add(exportMethodMetaData);
            }
            return exportMethodsInfo;
        }
    }
}
