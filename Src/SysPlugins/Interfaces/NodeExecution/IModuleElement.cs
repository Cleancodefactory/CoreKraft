using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces {
    public interface IModuleElement {
        /// <summary>
        /// Returns the name of the module where the definition originates
        /// </summary>
        string Module { get; }
    }
}
