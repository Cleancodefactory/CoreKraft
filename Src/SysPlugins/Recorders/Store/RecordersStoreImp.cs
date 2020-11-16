using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Concurrent;

namespace Ccf.Ck.SysPlugins.Recorders.Store
{
    /// <summary>
    /// Responsible for storing the recorders' references between requests
    /// </summary>
    public class RecordersStoreImp : IRequestRecordersStore
    {
        private static ConcurrentDictionary<string, IRequestRecorder> _RequestRecorders = new ConcurrentDictionary<string, IRequestRecorder>();

        public IRequestRecorder Get(string key)
        {
            IRequestRecorder result = null;
            _RequestRecorders.TryGetValue(key, out result);
            return result;
        }

        public void Remove(string key)
        {
            IRequestRecorder result = null;
            _RequestRecorders.TryRemove(key, out result);
        }

        public void Set(IRequestRecorder requestRecorder, string key)
        {
            Remove(key);
            _RequestRecorders.TryAdd(key, requestRecorder);
        }
    }
}
