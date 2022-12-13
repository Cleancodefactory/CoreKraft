using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.Settings {
    /// <summary>
    /// Configures a callscheduler event handler. Virtually all the settings are used for creating a DirectCall input model.
    /// </summary>
    public class CallScheduerHandler {
        public CallScheduerHandler() { }
        public CallScheduerHandler(CallScheduerHandler _handler) { 
            if (_handler != null ) {
                this.RunAs = _handler.RunAs;
                this.Address = _handler.Address;
                this.IsWriteOperation= _handler.IsWriteOperation;
            }
            
        }
        public string RunAs { get; set; }
        /// <summary>
        /// Use ParseCallAddress from Utilities.Generic to parse it into InputModel
        /// </summary>
        public string Address { get; set; }
        public bool IsWriteOperation { get; set; }

        #region Helpers
        public static CallScheduerHandler Read(string address, string runas = null) {
            return new CallScheduerHandler() { Address= address, RunAs = runas };
        }
        public static CallScheduerHandler Write(string address, string runas = null) {
            return new CallScheduerHandler() { Address = address, RunAs = runas, IsWriteOperation = true };
        }
        #endregion
    }
}
