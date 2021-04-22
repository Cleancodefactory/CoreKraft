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

        public IDictionary<string, IDependable<KraftDependableModule>> Dependencies { get; set; }
        public KraftModuleRootConf KraftModuleRootConf { get; set; }

        

        //internal IDependable<KraftModule> GetModuleAsDependable(string key)
        //{
        //    string pkey = ConstructValidKey(key);
        //    if (_KraftModulesCollection.ContainsKey(pkey))
        //    {
        //        return _KraftModulesCollection[pkey];
        //    }
        //    return null;
        //}
    }
}
