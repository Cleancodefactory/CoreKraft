using static Ccf.Ck.SysPlugins.Interfaces.Packet.StatusResultEnum;

namespace Ccf.Ck.SysPlugins.Interfaces.Packet
{
    public interface IStatusResult
    {
        string Message { get; set; }
        EStatusResult StatusResultType { get; set; }
    }

}
