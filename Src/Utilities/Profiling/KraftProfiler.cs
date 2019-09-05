using System;

namespace Ccf.Ck.Utilities.Profiling
{
    public class KraftProfiler
    {
        private static readonly Lazy<KraftProfiler> _Instance = new Lazy<KraftProfiler>(() => new KraftProfiler());

        // constructor private so users can't instantiate on their own
        private KraftProfiler()
        {
        }

        public static KraftProfiler Current
        {
            get
            {
                return _Instance.Value;
            }
        }
    }
}
