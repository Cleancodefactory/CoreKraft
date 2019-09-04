using System.Collections.Generic;

namespace Ccf.Ck.NodePlugins.BindKraftIntro.Models
{
    public class Sources
    {
        public Sources()
        {
            Entries = new List<SourceEntry>();
        }

        public List<SourceEntry> Entries { get; set; }
    }

    public enum ESourceType
    {
        HTML = 1,
        JAVASCRIPT = 2,
        JSON = 3,
        DOCUMENTATION = 4,
        PROPERTIES = 5
    }
}
