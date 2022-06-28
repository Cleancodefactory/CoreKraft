using System.Collections.Generic;

namespace Ccf.Ck.Models.NodeSet
{
    public class NodeSet
    {
        public NodeSet()
        {
            Parameters = new List<Parameter>();
        }

        public Node Root
        {
            get;
            set;
        }

        public List<Parameter> Parameters
        {
            get;
            set;
        }

        public bool RequireAuthentication
        {
            get;
            set;
        }

        public string DataPluginName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
    }
}