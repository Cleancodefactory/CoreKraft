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
            Started = DateTime.UtcNow;
        }
        public int Steps { get; protected set; }

        public EMetaInfoFlags Flags { get; protected set; }

        public MetaNode Root { get; protected set; }

        internal int AddStep() {
            return (++Steps);
        }
        public DateTime Started { get; protected set; }
        public DateTime Finished { get; protected set; }

        public TimeSpan ExecutionTime {
            get {
                return Finished - Started;
            }
        }

        public void SetFinished() { 
            Finished = DateTime.UtcNow;
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
