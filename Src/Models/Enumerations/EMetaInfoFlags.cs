using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.Enumerations {
    [Flags]
    public enum EMetaInfoFlags {
        None = 0x0000,  // With or without flags a crucial data has to be reported. This data can back non-optional features involved in the nodeset execution
        Basic = 0x0001, // Minimal execution information, preferably usable by optional features. E.g. information about fetched data which can be used by integral parts of the nodeset (but it must be optional)
        Trace = 0x0002, // report trace outputs. Usually special prints, constructed queries etc.
        Debug = 0x0004, // report debugging info - debug prints, execution logs and tracking. Most often Trace and Debug are given together
        Profile = 0x0010, // report profiling (timing, memory and similar)
        Log = 0x0008,    // report dedicated logging info - totally separate lane from what's reported on Trace and Debug.
        Output = 0x1000  // Flag considered by the response builders - if set they should serialize out the meta info (if they are of kind that can)
    }
}
