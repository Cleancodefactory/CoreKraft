using Ccf.Ck.Models.Enumerations;
using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public abstract class ActionQueryHostBase
    {
        public static bool IsTruthyOrFalsy(ParameterResolverValue v)
        {
            if (v.ValueType == EResolverValueType.ValueType || v.ValueType == EResolverValueType.ContentType)
            {
                // TODO: Redo this with converter to cover all types. Currently other types are unlikely to happen.
                if (v.Value == null) return false;
                if (v.Value is int i) return i != 0;
                if (v.Value is uint ui) return ui != 0;
                if (v.Value is double d) return d != 0;
                if (v.Value is long l) return l != 0;
                if (v.Value is ulong ul) return ul != 0;
                if (v.Value is short sh) return sh != 0;
                if (v.Value is ushort ush) return ush != 0;
                if (v.Value is char ch) return ch != 0;
                if (v.Value is byte bt) return bt != 0;
                if (v.Value is bool b) return b;
                if (v.Value is string s) return !string.IsNullOrWhiteSpace(s);
                return true;
            }
            else if (v.ValueType == EResolverValueType.Invalid || v.ValueType == EResolverValueType.Skip)
            {
                return false;
            }
            return false;
        }

    }
    
}
