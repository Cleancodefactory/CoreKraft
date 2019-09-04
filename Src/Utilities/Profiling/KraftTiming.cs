using StackExchange.Profiling;
using System;

namespace Ccf.Ck.Utilities.Profiling
{
    public class KraftTiming : IDisposable
    {
        bool _Disposed = false;
        Timing _Timing;
        public KraftTiming(Timing timing)
        {
            _Timing = timing;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
                return;

            if (disposing)
            {
                if (_Timing != null)
                {
                    (_Timing as IDisposable).Dispose();
                }
                // Free any other managed objects here.
            }

            // Free any unmanaged objects here.
            _Disposed = true;
        }
    }
}
