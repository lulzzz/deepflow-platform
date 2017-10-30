using System;
using System.Diagnostics;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Series;
using Deepflow.Platform.Sources.FakeSource.Data;
using Microsoft.Extensions.Logging;

namespace Deepflow.Sandbox.FakeSourcePerf
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataGenerator = new DeterministicDataGenerator(new Logger<RangeJoiner<RawDataRange>>());

            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                stopwatch = Stopwatch.StartNew();
                dataGenerator.GenerateRange("test", new TimeRange(DateTime.UtcNow.Subtract(TimeSpan.FromDays(365)), DateTime.UtcNow), 300);
                Console.WriteLine($"{stopwatch.ElapsedMilliseconds} ms");
            }
            
            //dataGenerator.GenerateRawPoint("test", (int)DateTime.UtcNow.SecondsSince1970Utc(), 300);

            Console.ReadKey();
        }
    }

    class Logger<T> : ILogger<T>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine(formatter(state, exception));
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
