namespace Ccf.Ck.Models.Settings.Modules
{
    public class KraftModuleSignal
    {
        public string Key { get; set; }
        public string NodeSet { get; set; }
        public string NodePath { get; set; }
        public bool Maintenance { get; set; }

        public string RunAs { get; set; }
    }
}
