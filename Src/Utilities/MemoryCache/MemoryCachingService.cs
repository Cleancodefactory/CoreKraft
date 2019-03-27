using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Ccf.Ck.Utilities.MemoryCache
{
    public class MemoryCachingService : ICachingService
    {
        private IMemoryCache _MemoryCache;
        public MemoryCachingService(IMemoryCache memoryCache)
        {
            if (memoryCache == null)
            {
                throw new Exception(nameof(memoryCache));
            }
            _MemoryCache = memoryCache;
        }
        public T Get<T>(string itemKey)
        {
            return _MemoryCache.Get<T>(itemKey);
        }

        public void Insert<T>(string itemKey, T content, IChangeToken changeToken = null)
        {
            if (changeToken != null)
            {
                _MemoryCache.Set<T>(itemKey, content, changeToken);
                return;
            }
            _MemoryCache.Set<T>(itemKey, content);
        }

        public void Remove(string itemKey)
        {
            _MemoryCache.Remove(itemKey);
        }

    }
}
