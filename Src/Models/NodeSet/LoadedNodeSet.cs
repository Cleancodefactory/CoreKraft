namespace Ccf.Ck.Models.NodeSet
{
    public class LoadedNodeSet
    {
        public NodeSet NodeSet
        {
            get;
            set;
        }

        public Node StartNode
        {
            get;
            set;
        }
        public Security GetSecurity() {
            if (NodeSet != null) {
                return Security.From(NodeSet.Security); // Makes a copy
            }
            return null;
        }
    }
}