using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Ccf.Ck.Launchers.Main.Utils
{
    public class RazorAssemblyLoadContext : AssemblyLoadContext
    {
        public RazorAssemblyLoadContext() : base(isCollectible: true)
        {
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }
    }
}
