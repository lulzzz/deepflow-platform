using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Extensions;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Series;
using Deepflow.Platform.Sources.FakeSource.Data;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Deepflow.Platform.Sandbox.InMemoryScatter
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var generator = new DeterministicDataGenerator(new Logger<RangeJoiner<RawDataRange>>());

            var start = new DateTime(2007, 1, 1).SecondsSince1970Utc();
            var end = new DateTime(2017, 1, 1).SecondsSince1970Utc();

            var allStopwatch = Stopwatch.StartNew();
            var generateStopwatch = Stopwatch.StartNew();
            var one = generator.GenerateData("one", new TimeRange(start, end), 300);
            var two = generator.GenerateData("two", new TimeRange(start, end), 300);
            var oneData = one.Data;
            var twoData = two.Data;
            Console.WriteLine("Generate " + generateStopwatch.ElapsedMilliseconds);

            ConfigurationOptions options = ConfigurationOptions.Parse("54.206.35.185,password=305bdaabb4c1881f2af5fdf68323a33d72ce87811c1663d503ac459ed7bd52dd");
            options.SyncTimeout = 300000;
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);
            IDatabase db = redis.GetDatabase();
            db.KeyDelete("one");
            db.KeyDelete("two");
            var saveStopwatch = Stopwatch.StartNew();
            
            db.StringSet("one", SerialiseData(one.Data));
            db.StringSet("two", SerialiseData(one.Data));
            Console.WriteLine("Save " + saveStopwatch.ElapsedMilliseconds);

            var loadStopwatch = Stopwatch.StartNew();
            oneData = DeserialiseData(db.StringGet("one"));
            twoData = DeserialiseData(db.StringGet("two"));
            Console.WriteLine("Load " + loadStopwatch.ElapsedMilliseconds);

            var width = 600;
            var height = 400;

            var joinStopwatch = Stopwatch.StartNew();
            var joined = JoinData(oneData, twoData).ToArray();
            Console.WriteLine("Join " + joinStopwatch.ElapsedMilliseconds);
            Console.WriteLine("Points " + joined.Length);

            var measureStopwatch = Stopwatch.StartNew();
            var minOne = double.MaxValue;
            var maxOne = double.MinValue;
            var minTwo = double.MaxValue;
            var maxTwo = double.MinValue;

            foreach (var joinedDatum in joined)
            {
                minOne = Math.Min(minOne, joinedDatum.OneValue);
                maxOne = Math.Max(maxOne, joinedDatum.OneValue);
                minTwo = Math.Min(minTwo, joinedDatum.TwoValue);
                maxTwo = Math.Max(maxTwo, joinedDatum.TwoValue);
            }
            var oneRange = maxOne - minOne;
            var twoRange = maxTwo - minTwo;
            Console.WriteLine("Measure " + measureStopwatch.ElapsedMilliseconds);

            var flattenStopwatch = Stopwatch.StartNew();
            var already = new HashSet<ulong>();
            foreach (var joinedDatum in joined)
            {
                var oneFloored = Math.Floor((joinedDatum.OneValue - minOne) / oneRange * width);
                var twoFloored = Math.Floor((joinedDatum.TwoValue - minTwo) / twoRange * height);

                var key = Interleave(oneFloored, twoFloored);
                already.Add(key);
            }
            Console.WriteLine("Flatten " + flattenStopwatch.ElapsedMilliseconds);
            Console.WriteLine("Flattened Points " + already.Count);

            Console.WriteLine("All " + allStopwatch.ElapsedMilliseconds);

            Console.ReadKey();*/
        }

        private static IEnumerable<JoinedDatum> JoinData(List<double> oneData, List<double> twoData)
        {
            if (oneData.Count == 0 || twoData.Count == 0)
            {
                yield break;
            }

            var joinedDatum = new JoinedDatum();
            var oneIndex = 0;
            var twoIndex = 0;
            double oneTime = oneData[oneIndex * 2];
            double twoTime = twoData[twoIndex * 2];
            while (oneIndex < oneData.Count && twoIndex < twoData.Count)
            {
                while (true)
                {
                    if (oneIndex == oneData.Count / 2)
                    {
                        yield break;
                    }

                    if ((oneTime = oneData[oneIndex * 2]) < twoTime)
                    {
                        oneIndex++;
                    }
                    break;
                }

                while (true)
                {
                    if (twoIndex == twoData.Count / 2)
                    {
                        yield break;
                    }

                    if ((twoTime = twoData[twoIndex * 2]) < oneTime)
                    {
                        twoIndex++;
                    }
                    break;
                }

                if (oneTime == twoTime)
                {
                    var oneValue = oneData[oneIndex * 2 + 1];
                    var twoValue = twoData[twoIndex * 2 + 1];

                    joinedDatum.Time = oneTime;
                    joinedDatum.OneValue = oneValue;
                    joinedDatum.TwoValue = twoValue;

                    yield return joinedDatum;

                    oneIndex++;
                    twoIndex++;
                }
            }
        }

        private static byte[] SerialiseData(List<double> data)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(data.Count * 8);
                    for (int i = 0; i < data.Count / 2; i++)
                    {
                        writer.Write(data[i * 2]);
                        writer.Write(data[i * 2 + 1]);
                    }
                }
                return stream.ToArray();
            }
        }

        private static List<double> DeserialiseData(byte[] bytes)
        {
            var data = new List<double>();

            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var bytesLength = reader.ReadInt32();
                    for (int i = 0; i < bytesLength / 16; i++)
                    {
                        data.Add(reader.ReadDouble());
                        data.Add(reader.ReadDouble());
                    }
                }
            }

            return data;
        }

        private struct JoinedDatum
        {
            public double Time;
            public double OneValue;
            public double TwoValue;
        }


        private static ulong Interleave(double xInt, double yInt)
        {
            ulong x = (ulong) xInt;
            ulong y = (ulong) yInt;

            x = (x | (x << 16)) & 0x0000FFFF0000FFFF;
            x = (x | (x << 8)) & 0x00FF00FF00FF00FF;
            x = (x | (x << 4)) & 0x0F0F0F0F0F0F0F0F;
            x = (x | (x << 2)) & 0x3333333333333333;
            x = (x | (x << 1)) & 0x5555555555555555;

            y = (y | (y << 16)) & 0x0000FFFF0000FFFF;
            y = (y | (y << 8)) & 0x00FF00FF00FF00FF;
            y = (y | (y << 4)) & 0x0F0F0F0F0F0F0F0F;
            y = (y | (y << 2)) & 0x3333333333333333;
            y = (y | (y << 1)) & 0x5555555555555555;

            return (x | (y << 1));
        }
    }

    public class Logger<T> : ILogger<T>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
