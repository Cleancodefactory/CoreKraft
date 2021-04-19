using Ccf.Ck.Models.Resolvers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public struct SymbolEntry
    {
        public SymbolEntry(string name, ParameterResolverValue val)
        {
            Name = name;
            Value = val;
        }

        public string Name;
        public ParameterResolverValue Value;

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return $"[*unnamed* {Value}]";
            } 
            else
            {
                return $"[{Name}={Value}]";
            }
        }
    }
}
