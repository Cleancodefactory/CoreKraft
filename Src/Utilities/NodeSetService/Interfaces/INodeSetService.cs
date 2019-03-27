using Ccf.Ck.Models.NodeSet;

namespace Ccf.Ck.Utilities.NodeSetService
{
    public interface INodeSetService
    {
        /* TODO: this is only the JSON implementation, must be extended to support more sources */
        LoadedNodeSet LoadNodeSet(string module, string treeNodesName, string nodeName);
    }
}
