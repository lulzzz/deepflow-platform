/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Net;
using Microsoft.Extensions.Logging;

namespace Deepflow.Ingestion.Service.Metrics
{
    public class MetricsReporter : IMetricsReporter
    {
        private readonly ILogger<TimerReporter> _logger;
        private readonly InfluxDb _influxDb;

        public MetricsReporter(ILogger<TimerReporter> logger)
        {
            _logger = logger;
            _influxDb = new InfluxDb("http://54.252.216.203:8086", "root", "root");
        }

        /*public IMetricsTimerReporter CreateTimerReporter(string name)
        {
            return new TimerReporter(name, _influxDb);
        }#1#

        public async Task<T> Run<T>(string name, Func<Task<T>> task)
        {
            var trip = new TimerReporter(name, _influxDb);
            var result = await task();
            trip.Dispose();
            return result;
        }

        public async Task Run(string name, Func<Task> task)
        {
            var trip = new TimerReporter(name, _influxDb);
            await task();
            trip.Dispose();
        }

        public async Task Run(string name, Task task)
        {
            var trip = new TimerReporter(name, _influxDb);
            await task;
            trip.Dispose();
        }

        public async Task Run(string name, IEnumerable<Task> tasks)
        {
            var trip = new TimerReporter(name, _influxDb);
            await Task.WhenAll(tasks);
            trip.Dispose();
        }

        public async Task<T> Run<T>(string name, Task<T> task)
        {
            var trip = new TimerReporter(name, _influxDb);
            var result = await task;
            trip.Dispose();
            return result;
        }
    }
}
*/
