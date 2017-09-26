using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Deepflow.Platform.Core.Async;

namespace Deepflow.Platform.Core.Caching
{
    public class ReadWriteThroughCache<T>
    {
        private readonly ConcurrentDictionary<string, AsyncLazy<T>> _cache = new ConcurrentDictionary<string, AsyncLazy<T>>();

        public Task<T> Read(string key, Func<string, Task<T>> getter)
        {
            return _cache.GetOrAdd(key, new AsyncLazy<T>(() => getter(key))).Value;
        }
    }
}
