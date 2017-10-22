using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Core.Tools
{
    public class TripCounter : IDisposable
    {
        private static readonly ConcurrentDictionary<string, Counter> Counters = new ConcurrentDictionary<string, Counter>();
        private readonly Counter _counter;

        public TripCounter(string name, ILogger<TripCounter> logger)
        {
            _counter = Counters.GetOrAdd(name, n => new Counter(name, logger));
            Interlocked.Increment(ref _counter.Count);
            Interlocked.Increment(ref _counter.Total);
        }

        public void Dispose()
        {
            Interlocked.Decrement(ref _counter.Count);
        }

        private class Counter
        {
            public int Count;
            public int Total;

            public Counter(string name, ILogger<TripCounter> logger)
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        logger.LogInformation($"{name} currently has {Count} inside and has counted {Total}");
                        await Task.Delay(3000);
                    }
                });
            }
        }
    }
}
