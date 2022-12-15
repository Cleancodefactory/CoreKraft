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
                    this.OnCallScheduled = new CallSchedulerHandler(_handlers.OnCallScheduled);
                }
                if (_handlers.OnCallStarted != null) {
                    this.OnCallStarted = new CallSchedulerHandler(_handlers.OnCallStarted);
                }
                if (_handlers.OnCallFinished!= null) {
                    this.OnCallFinished = new CallSchedulerHandler(_handlers.OnCallFinished);
                }
            }
                
            
            
        }

        public CallSchedulerHandler OnCallScheduled { get; set; }
        public CallSchedulerHandler OnCallStarted { get; set; }
        public CallSchedulerHandler OnCallFinished { get; set; }

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
