using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Deepflow.Platform.Core.Tools
{
    public sealed class AsyncCollection<T>
    {
        // The underlying collection of items.
        private readonly IProducerConsumerCollection<T> collection;

        // The maximum number of items allowed.
        private readonly int maxCount;

        // Synchronization primitives.
        private readonly AsyncLock mutex;
        private readonly AsyncConditionVariable notFull;
        private readonly AsyncConditionVariable notEmpty;

        public AsyncCollection(IProducerConsumerCollection<T> collection = null, int maxCount = int.MaxValue)
        {
            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("maxCount", "The maximum count must be greater than zero.");
            this.collection = collection ?? new ConcurrentQueue<T>();
            this.maxCount = maxCount;

            mutex = new AsyncLock();
            notFull = new AsyncConditionVariable(mutex);
            notEmpty = new AsyncConditionVariable(mutex);
        }

        // Convenience properties to make the code a bit clearer.
        private bool Empty { get { return collection.Count == 0; } }
        private bool Full { get { return collection.Count == maxCount; } }

        public async Task AddAsync(T item)
        {
            using (await mutex.LockAsync())
            {
                while (Full)
                    await notFull.WaitAsync();

                if (!collection.TryAdd(item))
                    throw new InvalidOperationException("The underlying collection refused the item.");
                notEmpty.Notify();
            }
        }

        public async Task<T> TakeAsync()
        {
            using (await mutex.LockAsync())
            {
                while (Empty)
                    await notEmpty.WaitAsync();

                T ret;
                if (!collection.TryTake(out ret))
                    throw new InvalidOperationException("The underlying collection refused to provide an item.");
                notFull.Notify();
                return ret;
            }
        }
    }
}
