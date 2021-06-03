using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.SysPlugins.Utilities {
    public class ValueList<T>:List<T> {
        public ValueList(): base() { }
        public ValueList(int capacity) : base(capacity) { }
        public ValueList(IEnumerable<T> enema) : base(enema) { }
    }
}
