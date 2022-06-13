using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectCall = Ccf.Ck.Models.DirectCall;

namespace Ccf.Ck.SysPlugins.Interfaces {
    public interface IIndirectCallInfo {
        public IndirectCallStatus Status { get; }
        public DirectCall.ReturnModel Result { get; }
        public DateTime? Started { get;}
        public DateTime? Finished { get;}
        public Guid ScheduleId { get; }

    }
}
