using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces {
    public interface ISupportsPluginServiceManager {
        IPluginServiceManager PluginServiceManager { get; }
    }
}
