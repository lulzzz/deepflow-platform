using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series.Providers
{
    public class QuantisedRandomDataGenerator
    {
        private static readonly SHA1 Sha1 = SHA1.Create();
        private readonly RangeFilterer<AggregatedDataRange> _filterer = new RangeFilterer<AggregatedDataRange>(new AggregatedRangeCreator(), new AggregateRangeFilteringPolicy(), new AggregatedRangeAccessor());

        public AggregatedDataRange GenerateData(string name, TimeRange timeRange, double minValue, double maxValue, double maxVariance, int aggregationSeconds, int quantisedInterval)
        {
            var multiple = quantisedInterval / (decimal)aggregationSeconds;
            if (multiple % 1 != 0)
            {
                throw new Exception("Quantised interval must be a multiple of the aggregation interval");
            }
            
            var quantisedTimeRange = timeRange.Quantise(quantisedInterval);
            var quantisedSpan = (int) (quantisedTimeRange.Max - quantisedTimeRange.Min);
            var chunks = quantisedSpan / quantisedInterval;
            var points = quantisedSpan / aggregationSeconds;
            var pointsPerChunk = quantisedInterval / aggregationSeconds;
            var data = new double[points * 2];
            
            for (int i = 0; i < chunks; i++)
            {
                var startPoint = i * pointsPerChunk;
                var endPoint = (i + 1) * pointsPerChunk;
                var startIndex = startPoint * 2;
                var endIndex = endPoint * 2;
                var startTime = quantisedTimeRange.Min + startPoint * aggregationSeconds;
                var endTime = quantisedTimeRange.Min + endPoint * aggregationSeconds;
                GenerateChunk(name, startTime, endTime, startIndex, endIndex, data, minValue, maxValue, maxVariance, aggregationSeconds);
            }
            
            var all = new AggregatedDataRange(quantisedTimeRange, data.ToList(), aggregationSeconds);
            return _filterer.FilterDataRange(all, timeRange);
        }

        private void GenerateChunk(string name, double minTime, double maxTime, int startIndex, int endIndex, double[] data, double minValue, double maxValue, double maxVariance, int aggregationSeconds)
        {
            var startTimeHash = Hash($"${name}_${minTime}_${minValue}_${maxValue}");
            var endTimeHash = Hash($"${name}_${maxTime}_${minValue}_${maxValue}");
            var fullHash = Hash($"{name}_${minTime}_${maxTime}_${maxVariance}_${aggregationSeconds}");

            Random random = new Random(fullHash);
            var valueRange = maxValue - minValue;
            var startValue = new Random(startTimeHash).NextDouble() * valueRange + minValue;
            var endValue = new Random(endTimeHash).NextDouble() * valueRange + minValue;
            
            SplitValueRecursive(minTime, startValue, maxTime, endValue, startIndex, endIndex, maxVariance, data, aggregationSeconds, random);
            data[endIndex - 2] = maxTime;
            data[endIndex - 1] = endValue;
        }

        private void SplitValueRecursive(double startTime, double startValue, double endTime, double endValue, int startIndex, int endIndex, double maxVariance, double[] data, int aggregationSeconds, Random random)
        {
            var range = endTime - startTime;
            var totalTimespans = range / aggregationSeconds;
            var midTime = startTime + Math.Floor(totalTimespans / 2) * aggregationSeconds;
            var midIndex = (int)Math.Floor((decimal)(startIndex + (endIndex - startIndex) / 2) / 2) * 2;

            if (totalTimespans <= 1)
            {
                return;
            }

            var midValue = startValue + (endValue - startValue) / 2;
            var randomisedMidValue = midValue + random.NextDouble() * maxVariance * 2 - maxVariance;

            SplitValueRecursive(startTime, startValue, midTime, randomisedMidValue, startIndex, midIndex, maxVariance / 1.5, data, aggregationSeconds, random);
            data[midIndex - 2] = midTime;
            data[midIndex - 1] = randomisedMidValue;
            SplitValueRecursive(midTime, randomisedMidValue, endTime, endValue, midIndex, endIndex, maxVariance / 1.5, data, aggregationSeconds, random);
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
