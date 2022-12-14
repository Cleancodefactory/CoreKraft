using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.Enumerations {
    /// <summary>
    /// Describes the kind of request being processed - web request, direct call, signal etc.
    /// The values are used for the REQUEST_CALL_TYPE constant.
    /// 
    /// FEATURE GENERAL
    /// 
    /// In the ServerVariables some special values can be set to identify the kind and purpose of the current call into CK.
    /// This enables plugins to use parameters fed from these variables and perform conditional actions depending on the conditions and 
    /// purpose of the current call. Example scenario: During import data is pushed into the database, but in normal operation the same data
    /// can be also managed by users. The same queries can apply different filters or assign different values to some fields during import and 
    /// user interaction.
    /// </summary>
    public enum ECallType {
        /// <summary>
        /// Typical WEB request
        /// </summary>
        WebRequest = 0,
        /// <summary>
        /// General internal direct call
        /// </summary>
        DirectCall = 1,
        /// <summary>
        /// Signal processing (internally can be a request or direct call)
        /// </summary>
        Signal = 2,
        /// <summary>
        /// Indirect scheduled call (implemented using direct call over service thread)
        /// </summary>
        ServiceCall = 3
    }

    public static class CallTypeConstants {
        /// <summary>
        /// Defines the Call type see the enumeration ECallType, Value: int, must be a number from the enum.
        /// </summary>
        public const string REQUEST_CALL_TYPE = "REQUEST_CALL_TYPE";
        /// <summary>
        /// A name identifying the request processor origin of the call. Value: string
        /// </summary>
        public const string REQUEST_PROCESSOR = "REQUEST_PROCESSOR";
        /// <summary>
        /// Rather loose classification of the task performed.
        /// By default it should be "webrequest" for web requests, "call" for internal calls, values along the lines of "import", "management", "configuration" 
        /// should be set to indicate what is being done.
        /// </summary>
        public const string TASK_KIND = "TASK_KIND";

        public const string TASK_KIND_WEBREQUEST = "webrequest";
        public const string TASK_KIND_CALL = "call";
        public const string TASK_KIND_CALLBACK = "callback";

    }
}
