/*
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Net;
using InfluxDB.Net.Enums;
using InfluxDB.Net.Infrastructure.Influx;
using InfluxDB.Net.Models;

namespace Deepflow.Ingestion.Service.Metrics
{
    public class TimerReporter : IMetricsTimerReporter
    {
        private static readonly ConcurrentDictionary<string, Counter> Counters = new ConcurrentDictionary<string, Counter>();
        private readonly Counter _counter;
        private readonly Stopwatch _stopwatch;
        private const int _interval = 3000;
        private const int _intervalSeconds = _interval / 1000;

        public TimerReporter(string name, InfluxDb influxDb)
        {
            _counter = Counters.GetOrAdd(name, n => new Counter(name, influxDb));
            _counter.Start();
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _counter.Stop((int)_stopwatch.ElapsedMilliseconds);
        }

        private class Counter
        {
            private Count Count = new Count();
            public int InsideCount;
            private InfluxDb _influxDb;

            public Counter(string name, InfluxDb influxDb)
            {
                _influxDb = influxDb;
                var metric = $"metric_{name}";
                Task.Run(async () =>
                {
                    while (true)
                    {
                        await Task.Delay(_interval);

                        var oldCount = Count;
                        Count = new Count();
                        //logger.LogInformation($"{name} currently has {InsideCount} inside and has counted {oldCount.Total} with average {(Total > 0 ? (float)Time / (float)Total : 0):0.00}");
                        var average = oldCount.Total > 0 ? oldCount.Time / oldCount.Total : 0;
                        var averagePerSecond = average / _intervalSeconds;
                        //_influxDb.WriteAsync("appmetricsdemo", new Point { Timestamp = DateTime.UtcNow, Fields = new Dictionary<string, object> { { "averagePerSecond", averagePerSecond }, { "inside", InsideCount } }, Measurement = metric, Precision = TimeUnit.Seconds });
                    }
                });
            }

            public void Start()
            {
                Interlocked.Increment(ref InsideCount);
            }

            public void Stop(int time)
            {
                var count = Count;
                Interlocked.Decrement(ref InsideCount);
                Interlocked.Increment(ref count.Total);
                Interlocked.Add(ref count.Time, time);
            }
        }

        private class Count
        {
            public int Total;
            public int Time;
        }
    }
}
*/
