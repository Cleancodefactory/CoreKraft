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
        /// <summary>
        /// Schedules a call for queued execution on a single (for the CoreKraft instnace) worker thread. The input (InputModel) is retained and passed to the
        /// call and fully available to the callee (nodeset). This is convenient, but also means the scheduled calls can keep considerable amount of memory 
        /// in the queue if some tasks need big amounts of data and the worker is overburdened with many tasks (the queue is quite full).
        /// </summary>
        /// <param name="input">The DirectCall.InputModel</param>1
        /// <param name="timeout">Schedulint timeout in seconds. This is a sanity limit which will cancel the task execution if it is not started until it ellapses.</param>
        /// <returns></returns>
        public Guid Call(DirectCall.InputModel input, int timeout = 86400);

        public IndirectCallStatus CallStatus(Guid guid);
        public DirectCall.ReturnModel GetResult(Guid guid);
    }
}
