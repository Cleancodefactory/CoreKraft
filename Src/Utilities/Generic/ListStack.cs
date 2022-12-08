using System;
using System.Collections.Generic;

namespace Ccf.Ck.Utilities.Generic
{
    public class ListStack<T> : List<T>
    {
        public ListStack() : base()
        {
        }
        public ListStack(int nc)
            : base(nc)
        {
        }
        public ListStack(IEnumerable<T> src)
            : base(src)
        {
        }

        public virtual T Pop()
        {
            if (Count > 0)
            {
                T t = this[Count - 1];
                RemoveAt(Count - 1);
                return t;
            }
            return default(T);
        }
        public virtual void Push(T t)
        {
            this.Add(t);
        }
        public virtual T Peek()
        {
            if (Count > 0)
            {
                return this[Count - 1];
            }
            return default(T);
        }
        public virtual T Top(int n = 0)
        {
            int i = this.Count - n;
            if (i > 0) return this[Count - n - 1];
            return default(T);
        }

        public StackFrame Scope(T framevalue) {
            return new StackFrame(this,framevalue);
        }

        public class StackFrame: IDisposable {
            private ListStack<T> _stack;

            public StackFrame(ListStack<T> stack,T framevalue) {
                if (stack == null) throw new ArgumentNullException();
                _stack = stack;
                _stack.Push(framevalue);
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing) {
                if (!disposedValue) {
                    if (disposing) {
                        _stack.Pop();
                    }

                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            // ~StackFrame() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //   Dispose(false);
            // }

            // This code added to correctly implement the disposable pattern.
            public void Dispose() {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }
            #endregion

        }
    }
}
