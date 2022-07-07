using Ccf.Ck.Models.NodeSet;
using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces.Packet
{
    public interface IReturnModel
    {
        object Data { get; set; }
        object LookupData { get; set; }
        object BinaryData { get; set; }
        IHttpResponseBuilder ResponseBuilder { get; set; }
        IReturnStatus Status { get; set; }
        Dictionary<string, IResourceModel> Views { get; set; }

        MetaRoot ExecutionMeta { get { return null; } }
    }
}
