using System.Collections.Generic;

namespace Ccf.Ck.SysPlugins.Interfaces.Packet
{
    public interface IReturnStatus
    {
        bool IsSuccessful { get; set; }
        List<IStatusResult> StatusResults { get; set; }
    }
}
