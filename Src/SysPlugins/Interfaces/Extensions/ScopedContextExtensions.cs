using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Interfaces {
    public static class ScopedContextExtensions {
        public static void SetPluginName(this IPluginsSynchronizeContextScoped scope, string name) {
            if (scope is IPluginsSynchronizeContextScopedEx scopeex) {
                scopeex.PluginName = name;
            }
        }
        public static void SetModuleName(this IPluginsSynchronizeContextScoped scope, string name) {
            if (scope is IPluginsSynchronizeContextScopedEx scopeex) {
                scopeex.ModuleName = name;
            }
        }
    }
}
