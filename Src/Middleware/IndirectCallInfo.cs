using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Web.Middleware {
    public record IndirectCallInfo(Guid ScheduleId, IndirectCallStatus Status, Models.DirectCall.ReturnModel Result = null, DateTime? Started = null, DateTime? Finished = null) : IIndirectCallInfo;
}
