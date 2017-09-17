using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series.Providers
{
    public class EnhancedRandomDataGenerator
    {
        private static readonly SHA1 Sha1 = SHA1.Create();

        public RawDataRange GenerateData(string name, long startTimeSeconds, long endTimeSeconds, double minValue, double maxValue, double maxVariance, TimeSpan maxTimespan)
        {
            var startTimeHash = Hash($"${name}_${startTimeSeconds}_${minValue}_${maxValue}");
            var endTimeHash = Hash($"${name}_${endTimeSeconds}_${minValue}_${maxValue}");
            var fullHash = Hash($"{name}_${startTimeSeconds}_${endTimeSeconds}_${maxVariance}_${maxTimespan}");

            Random random = new Random(fullHash);
            var valueRange = maxValue - minValue;
            var startValue = new Random(startTimeHash).NextDouble() * valueRange + minValue;
            var endValue = new Random(endTimeHash).NextDouble() * valueRange + minValue;
            var startDatum = new Datum { Time = startTimeSeconds, Value = startValue };
            var endDatum = new Datum { Time = endTimeSeconds, Value = endValue };

            List<Datum> data = new List<Datum>();
            data.Add(startDatum);

            if (startTimeSeconds == endTimeSeconds)
            {
                return ToDataRange((long) (startTimeSeconds - maxTimespan.TotalSeconds), endTimeSeconds, data);
            }

            SplitValueRecursive(startDatum, endDatum, maxVariance, data, maxTimespan, random);
            data.Add(endDatum);

            return ToDataRange(startTimeSeconds, endTimeSeconds, data);
        }

        private RawDataRange ToDataRange(long minSeconds, long maxSeconds, List<Datum> data)
        {
            var dataArray = new List<double>();
            var i = 0;
            foreach (var datum in data)
            {
                dataArray[i * 2] = datum.Time;
                dataArray[i * 2 + 1] = datum.Value;
                i++;
            }
            return new RawDataRange(minSeconds, maxSeconds, dataArray);
        }

        private void SplitValueRecursive(Datum start, Datum end, double maxVariance, List<Datum> data, TimeSpan maxTimespan, Random random)
        {
            var range = end.Time - start.Time;
            var totalTimespans = range / maxTimespan.TotalSeconds;
            var midTime = start.Time + Math.Floor(totalTimespans / 2) * maxTimespan.TotalSeconds;

            if (totalTimespans <= 1)
            {
                return;
            }

            var midValue = start.Value + (end.Value - start.Value) / 2;
            var randomisedMidValue = midValue + random.NextDouble() * maxVariance * 2 - maxVariance;

            var midDatum = new Datum { Time = midTime, Value = randomisedMidValue };

            SplitValueRecursive(start, midDatum, maxVariance / 1.5, data, maxTimespan, random);
            data.Add(midDatum);
            SplitValueRecursive(midDatum, end, maxVariance / 1.5, data, maxTimespan, random);
        }

        public static int Hash(string src)
        {
            byte[] stringbytes = Encoding.UTF8.GetBytes(src);
            byte[] hashedBytes = Sha1.ComputeHash(stringbytes);
            Array.Resize(ref hashedBytes, 4);
            return BitConverter.ToInt32(hashedBytes, 0);
        }
    }
}
