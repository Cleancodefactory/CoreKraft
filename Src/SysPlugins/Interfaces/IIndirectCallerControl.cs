using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectCall = Ccf.Ck.Models.DirectCall;

namespace Ccf.Ck.SysPlugins.Interfaces {
    public interface IIndirectCallerControl {
        IIndirectCallerInfo GetIndirectServiceInfo();
        IIndirectCallerThreads GetIndirectServiceThreadInfo();
    }
}
