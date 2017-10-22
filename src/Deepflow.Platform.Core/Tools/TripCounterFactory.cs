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

        public TripCounter Create(string name)
        {
            return new TripCounter(name, _logger);
        }

        public async Task Run(string name, Task task)
        {
            using (new TripCounter(name, _logger))
            {
                await task;
            }
        }

        public async Task Run(string name, IEnumerable<Task> tasks)
        {
            using (new TripCounter(name, _logger))
            {
                await Task.WhenAll(tasks);
            }
        }

        public async Task<T> Run<T>(string name, Task<T> task)
        {
            using (new TripCounter(name, _logger))
            {
                return await task;
            }
        }
    }
}