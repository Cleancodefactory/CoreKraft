using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ccf.Ck.SysPlugins.Utilities
{
    public class SymbolSet
    {
        private SymbolEntry[] _Entries;
        public string Name { get; private set; }

        public SymbolSet(string name, IEnumerable<SymbolEntry> entries)
        {
            if (entries != null) _Entries = entries.ToArray();
            Name = name;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Name} symbols:");
            sb.AppendLine(String.Join('\n', _Entries.Select(s => s.ToString())));
            return sb.ToString();
        }
    }
}
