using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings.Modules
{
    public class KraftModuleRootConf
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public List<string> Keywords { get; set; }
        public string Author { get; set; }
        public List<KraftModuleSignal> Signals { get; set; }
        public string License { get; set; }
        public Dictionary<string, string> Dependencies { get; set; }
        public Dictionary<string, string> OptionalDependencies { get; set; }
        public List<KraftModuleRelease> Release { get; set; }
    }
}
