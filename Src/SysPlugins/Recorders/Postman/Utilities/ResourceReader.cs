using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Ccf.Ck.SysPlugins.Recorders.Postman.Utilities
{
    class ResourceReader
    {
        public static string GetResource(string key)
        {
            Assembly assembly = typeof(PostmanImp).GetTypeInfo().Assembly;
            Stream resource = assembly.GetManifestResourceStream($"Ccf.Ck.SysPlugins.Recorders.Postman.Resources.{key}.json");
            using (var reader = new StreamReader(resource))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
