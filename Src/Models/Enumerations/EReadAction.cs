using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.Enumerations {
    /// <summary>
    /// Enumeration used in input models to specify what kind of action must be performed. Whe nothing is specified hte default action is "select"
    /// </summary>
    public enum EReadAction {
        Default = 0,
        Select = 0,
        New = 1
    }
}
