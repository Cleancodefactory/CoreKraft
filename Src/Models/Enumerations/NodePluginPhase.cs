using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.Enumerations {
    public enum NodePluginPhase {
        BeforeNode = 10,
        BeforeAction = 20,
        AfterAction = 30,
        AfterChildren = 40
    }
}
