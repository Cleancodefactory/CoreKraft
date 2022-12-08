using Ccf.Ck.Models.Web.Settings;
using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class CallSchedulerCallHandlers //: IConfigurationModel
    {

        public CallSchedulerCallHandlers()
        {
            
        }

        public CallScheduerHandler OnCallScheduled { get; set; }
        public CallScheduerHandler OnCallStarted { get; set; }
        public CallScheduerHandler OnCallFinished { get; set; }
    }
}
