using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Deepflow.Ingestion.Service.Metrics
{
    public class MetricsReporter : IMetricsReporter
    {
        private readonly ConcurrentDictionary<string, Metrics> _metrics = new ConcurrentDictionary<string, Metrics>();
        private readonly IPEndPoint _ipEndPoint;
        private readonly Socket _socket;
        private int _interval = 2000;

        public MetricsReporter(ILogger<MetricsReporter> logger)
        {
            var hostedGraphiteApiKey = "db058b3b-5455-424e-8e37-248ddf8173f6";

            _ipEndPoint = new IPEndPoint(Dns.GetHostAddresses("4326a9d9.carbon.hostedgraphite.com")[0], 2003);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { Blocking = true };
            
            Task.Run(async () =>
            {
                while (true)
                {
                    var stopwatch = Stopwatch.StartNew();
                    foreach (var metrics in _metrics)
                    {
                        var timing = Interlocked.Exchange(ref metrics.Value.Timing, new Timing());
                        var averageTime = timing.CountThisSecond == 0 ? 0 : timing.TotalTimeThisSecond / timing.CountThisSecond;
                        var averageTimeMessage = $"{hostedGraphiteApiKey}.{metrics.Key}.averagetime {averageTime}";
                        var activeMessage = $"{hostedGraphiteApiKey}.{metrics.Key}.active {metrics.Value.Active}";
                        SendMetric(averageTimeMessage);
                        SendMetric(activeMessage);
                    }

                    logger.LogWarning("Reported metrics");
                    var delay = _interval - (int)stopwatch.ElapsedMilliseconds;
                    if (delay > 0)
                    {
                        await Task.Delay(delay);
                    }
                }
            });
        }

        public async Task<T> Run<T>(string name, Func<Task<T>> task)
        {
            var metrics = _metrics.GetOrAdd(name, new Metrics());
            metrics.Active++;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await task();
            }
            finally
            {
                metrics.Active--;
                var time = stopwatch.ElapsedMilliseconds;
                metrics.Timing.TotalTimeThisSecond += time;
                metrics.Timing.CountThisSecond++;
            }
        }

        public async Task Run(string name, Func<Task> task)
        {
            var metrics = _metrics.GetOrAdd(name, new Metrics());
            metrics.Active++;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await task();
            }
            finally
            {
                metrics.Active--;
                var time = stopwatch.ElapsedMilliseconds;
                metrics.Timing.TotalTimeThisSecond += time;
                metrics.Timing.CountThisSecond++;
            }
        }

        public Task Run(string name, Task task)
        {
            return Run(name, task);
        }

        public async Task Run(string name, IEnumerable<Task> tasks)
        {
            await Task.WhenAll(tasks);
        }

        public Task<T> Run<T>(string name, Task<T> task)
        {
            return Run(name, () => task);
        }

        private class Metrics
        {
            public int Active;
            public Timing Timing = new Timing();
        }

        private class Timing
        {
            public int CountThisSecond;
            public long TotalTimeThisSecond;
        }

        private void SendMetric(string message)
        {
            var bytes = Encoding.ASCII.GetBytes($"{message}\n");
            _socket.SendTo(bytes, _ipEndPoint);
        }
    }
}
