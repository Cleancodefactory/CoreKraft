using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    public interface IIteratorMeta {
        MetaNode Child(string name);
    }
}
