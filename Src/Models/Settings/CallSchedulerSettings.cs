using Ccf.Ck.Models.Web.Settings;
using System.Collections.Generic;

namespace Ccf.Ck.Models.Settings
{
    public class CallSchedulerSettings //: IConfigurationModel
    {

        public CallSchedulerSettings()
        {
            
        }

        public CallScheduerHandler OnCallScheduled { get; set; }
        public CallScheduerHandler OnCallStarted { get; set; }
        public CallScheduerHandler OnCallFinished { get; set; }
    }
}
