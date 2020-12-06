using Ccf.Ck.SysPlugins.Interfaces;
using System.Collections.Concurrent;

namespace Ccf.Ck.SysPlugins.Recorders.Store
{
    /// <summary>
    /// Responsible for storing the recorders' references between requests
    /// </summary>
    public class RecordersStoreImp : IRequestRecordersStore
    {
        private static readonly ConcurrentDictionary<string, IRequestRecorder> _RequestRecorders = new ConcurrentDictionary<string, IRequestRecorder>();

        public IRequestRecorder Get(string key)
        {
            _RequestRecorders.TryGetValue(key, out IRequestRecorder result);
            return result;
        }

        public void Remove(string key)
        {
            _RequestRecorders.TryRemove(key, out _);
        }

        public void Set(IRequestRecorder requestRecorder, string key)
        {
            Remove(key);
            _RequestRecorders.TryAdd(key, requestRecorder);
        }
    }
}
