using Ccf.Ck.SysPlugins.Interfaces.Packet;
using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;

namespace Ccf.Ck.Models.Packet
{
    public class StatusResult : IStatusResult
    {
        public string Message { get; set; }
        public EStatusResult StatusResultType { get; set; }
    }
}
