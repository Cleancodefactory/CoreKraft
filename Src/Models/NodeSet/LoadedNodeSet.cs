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
        public Security GetNodeSetSecurity() {
            if (NodeSet != null) {
                return Security.From(NodeSet); // Makes a copy
            }
            return null;
        }
        public Security GetStartSecurity() {
            if (NodeSet != null) {
                Security security = Security.From(NodeSet.Security); // Makes a copy
                if (StartNode != null) {
                    security?.OverrideWith(StartNode.Security);
                }
                return security;
            } else {
                if (StartNode != null) {
                    Security security = Security.From(StartNode);
                    return security;
                }
                return null;
            }
        }
    }
}