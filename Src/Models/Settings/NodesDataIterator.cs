using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class NodesDataIterator
    {
        public NodesDataIterator()
        {
            NodesDataIteratorConf = new LoaderProperties();
        }

        public LoaderProperties NodesDataIteratorConf { get; set; }

        public List<LoaderProperties> NodesDataLoader { get; set; }
    }
}
