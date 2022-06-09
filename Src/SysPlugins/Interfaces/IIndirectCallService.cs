using System;
using DirectCall = Ccf.Ck.Models.DirectCall;

namespace Ccf.Ck.SysPlugins.Interfaces {

    public enum IndirectCallStatus {
        Unavailable = 0x0000,
        Queued = 0x0001,
        Running = 0x0002,
        Finished = 0x0003,
        Discarded = 0x0004    // The task was not executed, most likely because of a scheduling timeout
    }
    public interface IIndirectCallService {
        public Guid Call(DirectCall.InputModel input, int timeout);

        public IndirectCallStatus CallStatus(Guid guid);
        public DirectCall.ReturnModel GetResult(Guid guid);
    }
}
