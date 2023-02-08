using Ccf.Ck.Models.DirectCall;
using Ccf.Ck.SysPlugins.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Web.Middleware {
    public record IndirectCallInfo(Guid ScheduleId, IndirectCallStatus Status, Models.DirectCall.ReturnModel Result = null, DateTime? Started = null, DateTime? Finished = null) : IIndirectCallInfo;
    public record IndirectCallInfoEx(Guid ScheduleId, IndirectCallStatus Status, Models.DirectCall.InputModel Input = null, Models.DirectCall.ReturnModel Result = null, DateTime? Started = null, DateTime? Finished = null, DateTime? Scheduled = null) : IIndirectCallInfoEx;

    public record IndirectCallerInfo(IEnumerable<IIndirectCallInfoEx> Waiting, IEnumerable<IIndirectCallInfoEx> Finished): IIndirectCallerInfo;
    public record IndirectCallThreadInfo(IEnumerable<ThreadInfo> Threads) : IIndirectCallerThreads;
}

