using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly Stopwatch _stopwatch;

        public TripCounter(string name, ILogger<TripCounter> logger)
        {
            _counter = Counters.GetOrAdd(name, n => new Counter(name, logger));
            _counter.Start();
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _counter.Stop((int)_stopwatch.ElapsedMilliseconds);
        }

        private class Counter
        {
            public int Count;
            public int Total;
            public int Time;

            public Counter(string name, ILogger<TripCounter> logger)
            {
                Task.Run(async () =>
                {
                    while (true)
                    {
                        logger.LogInformation($"{name} currently has {Count} inside and has counted {Total} with average {(Total > 0 ? (float)Time / (float)Total : 0):0.00}");
                        await Task.Delay(3000);
                    }
                });
            }

            public void Start()
            {
                Interlocked.Increment(ref Count);
                Interlocked.Increment(ref Total);
            }

            public void Stop(int time)
            {
                Interlocked.Decrement(ref Count);
                Interlocked.Add(ref Time, time);
            }
        }
    }
}
