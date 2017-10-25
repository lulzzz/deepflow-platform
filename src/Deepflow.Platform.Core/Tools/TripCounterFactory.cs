using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Core.Tools
{
    public class TripCounterFactory
    {
        private readonly ILogger<TripCounter> _logger;

        public TripCounterFactory(ILogger<TripCounter> logger)
        {
            _logger = logger;
        }

        /*public TripCounter Run(string name, )
        {
            return new TripCounter(name, _logger);
        }*/

        public async Task Run(string name, Func<Task> task)
        {
            var trip = new TripCounter(name, _logger);
            await task();
            trip.Dispose();
        }

        public async Task Run(string name, Task task)
        {
            var trip = new TripCounter(name, _logger);
            await task;
            trip.Dispose();
        }

        public async Task Run(string name, IEnumerable<Task> tasks)
        {
            var trip = new TripCounter(name, _logger);
            await Task.WhenAll(tasks);
            trip.Dispose();
        }

        public async Task<T> Run<T>(string name, Task<T> task)
        {
            var trip = new TripCounter(name, _logger);
            var result = await task;
            trip.Dispose();
            return result;
        }
    }
}