using Ccf.Ck.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Models.NodeSet {
    public class MetaRoot: IIteratorMeta {
        public MetaRoot(EMetaInfoFlags flags = EMetaInfoFlags.None) {
            Steps = 0;
            Flags = flags;
        }
        public int Steps { get; protected set; }

        public EMetaInfoFlags Flags { get; protected set; }

        public MetaNode Root { get; protected set; }

        internal int AddStep() {
            return (++Steps);
        }

        //public MetaNode Current { get; protected set; }

        #region IIteratorMeta

        public MetaNode Child(string name) {
            if (Root == null) {
                Root = new MetaNode(this, name);
                return Root;
            } else {
                throw new InvalidOperationException("Second attempt to create root MetaNode");
            }
        }

        #endregion

    }
}
