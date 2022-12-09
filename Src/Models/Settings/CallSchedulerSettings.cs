using Ccf.Ck.Models.Web.Settings;
using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class CallSchedulerSettings //: IConfigurationModel
    {

        public CallSchedulerSettings()
        {
            
        }

        /// <summary>
        /// General callbacks applied for each call, similar to the ones specified in the InputModel
        /// </summary>
        public CallSchedulerCallHandlers CallHandlers { get; set; }
        /// <summary>
        /// Nodeset to call when the queue becomes empty - this call is actually scheduled and not executed immediately
        /// </summary>
        public CallScheduerHandler OnEmptyQueue { get; set; }
        /// <summary>
        /// Delays the que processing after the ap startup (to give it time to settle enough)
        /// </summary>
        public int StartupDelay { get; set; }
        public int RecheckSeconds { get; set; }
        public int ResultPreserveSeconds { get; set;}
        public int ScheduleTimeoutSeconds { get; set; }
        public int WorkerThreads { get; set;}
    }
}
