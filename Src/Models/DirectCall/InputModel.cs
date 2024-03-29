﻿using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Interfaces;
using Ccf.Ck.Models.Settings;
using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace Ccf.Ck.Models.DirectCall
{
    public class InputModel
    {
        public InputModel() {
        }
        public InputModel(string module, string nodeset, string nodepath, bool isWriteOperation= false, EReadAction readAction = EReadAction.Default) {
            Module = module;
            Nodeset = nodeset;
            Nodepath = nodepath;
            IsWriteOperation = isWriteOperation;
            ReadAction = readAction;
        }

        public string RunAs { get; set; }
        public string Module { get; set; }
        public string Nodeset { get; set; }
        public string Nodepath { get; set; }
        public bool IsWriteOperation { get; set; }
        public EReadAction ReadAction { get; set; } = EReadAction.Default;
        public ISecurityModel SecurityModel { get; set; }
        public Dictionary<string, object> QueryCollection { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public ECallType CallType { get; set; } = ECallType.DirectCall;
        public string TaskKind { get; set; } = CallTypeConstants.TASK_KIND_CALL;

        /// <summary>
        /// Callbacks to call before and after the call processing. Specify only those you want and leave the others null.
        /// </summary>
        public CallSchedulerCallHandlers SchedulerCallHandlers { get; set; }
    }
}
