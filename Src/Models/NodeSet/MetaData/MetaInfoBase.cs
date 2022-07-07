using Ccf.Ck.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    // Base class for all meta infos
    public class MetaInfoBase {
        
        public EMetaInfoFlags Flags { get; internal set; }
    }
}
