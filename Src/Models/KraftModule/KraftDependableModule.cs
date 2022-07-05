using Ccf.Ck.Models.Settings.Modules;
using Ccf.Ck.Utilities.Generic.Topologies;
using System;
using System.Collections.Generic;

namespace Ccf.Ck.Models.KraftModule
{
    public class KraftDependableModule : IDependable<KraftDependableModule>
    {
        public int DependencyOrderIndex { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public IDictionary<string, IDependable<KraftDependableModule>> Dependencies { get; set; }
        public KraftModuleRootConf KraftModuleRootConf { get; set; }
        public string KraftModuleRootPath { get; set; }
    }
}
