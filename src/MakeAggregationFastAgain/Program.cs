using System;
using System.Collections.Generic;
using System.Diagnostics;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Series;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Sandbox.MakeAggregationFastAgain
{
    class Program
    {
        static void Main(string[] args)
        {
            /*Console.WriteLine("Hello World!");

            var data = new List<double>();
            var minTime = new DateTime(2016, 1, 1).SecondsSince1970Utc();
            var maxTime = new DateTime(2016, 6, 1).SecondsSince1970Utc();
            var random = new Random();
            for (var seconds = minTime; seconds <= maxTime; seconds += 300)
            {
                data.Add(seconds);
                data.Add(random.NextDouble());
            }

            var dataRange = new RawDataRange(minTime, maxTime, data);

            DataAggregator aggregator = new DataAggregator(new Logger<DataAggregator>());

            Stopwatch stopwatch = Stopwatch.StartNew();
            var result = aggregator.Aggregate(dataRange, dataRange.TimeRange, 300);

            Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds} ms");
            Console.ReadKey();*/
        }
    }

    class Logger<T> : ILogger<T>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}
