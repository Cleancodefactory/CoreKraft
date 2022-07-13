using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.Enumerations {
    [Flags]
    public enum EMetaInfoFlags {
        None = 0x0000,
        Basic = 0x0001,
        Trace = 0x0002,
        Debug = 0x0004,
        Log = 0x0008
    }
}
