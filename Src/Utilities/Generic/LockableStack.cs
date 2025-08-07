using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccf.Ck.Utilities.Generic {
    public class LockableStack<T> : Stack<T> {

        public bool Locked { get; set; } = false;

        #region Construction
        public LockableStack() : base() { }
        public LockableStack(int capacity) : base(capacity) { }
        public LockableStack(IEnumerable<T> items) : base(items) { }
        #endregion Construction


        public new void Clear() {
            if (Locked) return;
            base.Clear();
        }
        public new void Push(T item) {
            if (Locked) return;
            base.Push(item);
        }

        public new T Pop() {
            if (Locked) {
                return this.Peek();
            } else {
                return base.Pop();
            }
        }
        public new bool TryPop(out T item) {
            if (Locked) {
                item = default;
                return false;
            } else {
                return base.TryPop(out item);
            }
        }
    }
}
