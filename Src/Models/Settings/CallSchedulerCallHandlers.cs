using Ccf.Ck.Models.Web.Settings;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Ccf.Ck.Models.Settings
{
    public class CallSchedulerCallHandlers //: IConfigurationModel
    {

        public CallSchedulerCallHandlers()
        {
            
        }
        CallSchedulerCallHandlers(CallSchedulerCallHandlers _handlers) :this() {
            if (_handlers != null) {
                if (_handlers.OnCallScheduled!= null) {
                    this.OnCallScheduled = new CallScheduerHandler(_handlers.OnCallScheduled);
                }
                if (_handlers.OnCallStarted != null) {
                    this.OnCallStarted = new CallScheduerHandler(_handlers.OnCallStarted);
                }
                if (_handlers.OnCallFinished!= null) {
                    this.OnCallFinished = new CallScheduerHandler(_handlers.OnCallFinished);
                }
            }
                
            
            
        }

        public CallScheduerHandler OnCallScheduled { get; set; }
        public CallScheduerHandler OnCallStarted { get; set; }
        public CallScheduerHandler OnCallFinished { get; set; }

        /// <summary>
        /// Returns a clone of this object if any handler is non-null and null otherwise
        /// </summary>
        /// <returns></returns>
        public CallSchedulerCallHandlers CloneOrNull() {
            if (OnCallScheduled != null || OnCallStarted != null || OnCallFinished != null) {
                return new CallSchedulerCallHandlers(this);
            }
            return null;
        }
    }
}
