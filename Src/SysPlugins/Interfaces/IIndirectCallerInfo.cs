using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectCall = Ccf.Ck.Models.DirectCall;

namespace Ccf.Ck.SysPlugins.Interfaces {
    public interface IIndirectCallerInfo {
        IEnumerable<IIndirectCallInfoEx> Waiting { get; }
        IEnumerable<IIndirectCallInfoEx> Finished { get; }

        IIndirectCallInfoEx Running {
            get {
                var tsk = Finished.FirstOrDefault(t => t.Status == IndirectCallStatus.Running);
                return tsk;
            }
        }
    }
}
