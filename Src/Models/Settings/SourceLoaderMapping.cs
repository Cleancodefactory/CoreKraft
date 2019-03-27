using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class SourceLoaderMapping
    {
        public SourceLoaderMapping()
        {
            NodesDataIterator = new NodesDataIterator();
            ViewLoader = new List<LoaderProperties>();
            LookupLoader = new List<LoaderProperties>();
            ResourceLoader = new List<LoaderProperties>();
            CustomPlugin = new List<LoaderProperties>();
        }
        public NodesDataIterator NodesDataIterator { get; set; }

        public List<LoaderProperties> ViewLoader { get; set; }

        public List<LoaderProperties> LookupLoader { get; set; }

        public List<LoaderProperties> ResourceLoader { get; set; }

        public List<LoaderProperties> CustomPlugin { get; set; }
    }
}
