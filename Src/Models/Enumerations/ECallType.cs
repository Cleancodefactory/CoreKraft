using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.Enumerations {
    /// <summary>
    /// Describes the kind of request being processed - web request, direct call, signal etc.
    /// </summary>
    public enum ECallType {
        WebRequest = 0,
        DirectCall = 1,
        Signal = 2,
        ServiceTask = 3
    }
}
