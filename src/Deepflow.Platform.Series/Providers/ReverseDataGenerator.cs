using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series.Providers
{
    public class ReverseDataGenerator
    {
        private readonly IValueGenerator _valueGenerator;

        public ReverseDataGenerator(IValueGenerator valueGenerator)
        {
            _valueGenerator = valueGenerator;
        }

        public AggregatedDataRange GenerateReverseAverage(string name, TimeRange timeRange, int[] aggregationLevels, int aggregationLevel, double minValue, double maxValue)
        {
            if (minValue <= 0)
            {
                throw new Exception("Can only generate random data above zero.");
            }

            var descendingAggregationLevels = aggregationLevels.OrderByDescending(x => x).ToArray();
            var highestAggregationLevel = descendingAggregationLevels[0];
            var lowestAggregationLevel = descendingAggregationLevels[descendingAggregationLevels.Length - 1];
            var aggregationLevelIndex = Array.IndexOf(descendingAggregationLevels, aggregationLevel);

            var quantisedLowTime = GetQuantisedTimeBeforeOrOn(highestAggregationLevel, timeRange.Min);
            var quantisedHighTime = GetQuantisedTimeAfterOrOn(highestAggregationLevel, timeRange.Max);

            var variance = (maxValue - minValue) / 10;
            
            var rangeValues = new LowestAggregationValues(quantisedLowTime, quantisedHighTime, lowestAggregationLevel);
            
            GenerateReverseAverageForRange(name, quantisedLowTime, quantisedHighTime, timeRange, descendingAggregationLevels, aggregationLevelIndex, minValue, maxValue, variance, rangeValues);

            List<double> data = new List<double>();
            for (var time = quantisedLowTime; time <= quantisedHighTime; time += aggregationLevel)
            {
                if (time < timeRange.Min)
                {
                    continue;
                }
                
                var value = rangeValues.GetValue(time);

                data.Add(time);
                data.Add(value);
            }
            
            return new AggregatedDataRange(timeRange, data, aggregationLevel);
        }

        private void GenerateReverseAverageForRange(string name, long lowTime, long highTime, TimeRange timeRange, int[] aggregationLevels, int maxAggregationLevelIndex, double minValue, double maxValue, double variance, LowestAggregationValues lowestAggregationValues)
        {
            // Generate min and max value
            var lowValue = _valueGenerator.GenerateValue(name, lowTime, minValue, maxValue);
            var highValue = _valueGenerator.GenerateValue(name, highTime, minValue, maxValue);

            // Add to data array
            lowestAggregationValues.AddValue(lowTime, lowValue);
            lowestAggregationValues.AddValue(highTime, highValue);

            // Generate child points
            GenerateReverseAverageRecursive(name, lowTime, highTime, highValue, timeRange, aggregationLevels, 1, maxAggregationLevelIndex, variance, lowestAggregationValues);
        }

        private void GenerateReverseAverageRecursive(string name, long lowTime, long highTime, double highValue, TimeRange timeRange, int[] aggregationLevels, int aggregationLevelIndex, int maxAggregationLevelIndex, double variance, LowestAggregationValues lowestAggregationValues)
        {
            if (aggregationLevelIndex > maxAggregationLevelIndex)
            {
                return;
            }

            if (lowTime > timeRange.Max || highTime < timeRange.Min)
            {
                return;
            }

            var aggregationLevel = aggregationLevels[aggregationLevelIndex];
            var nextAggregationIndex = aggregationLevelIndex + 1;
            var nextAggregationLevel = aggregationLevels[nextAggregationIndex];
            var aggregationMultiplier = nextAggregationLevel / aggregationLevel;
            var nextVariance = aggregationMultiplier * variance;

            var childTimes = GetQuantisedTimesBetweenOrAtEnd(aggregationLevel, lowTime, highTime);
            foreach (var childTime in childTimes)
            {
                var childBeforeTime = childTime - aggregationLevel;
                var childBeforeValue = lowestAggregationValues.GetValue(childBeforeTime);
                
                var childValue = lowestAggregationValues.GetOrAddValue(childTime, () => _valueGenerator.GenerateValueBetween(name, childTime, childBeforeTime, highTime, childBeforeValue, highValue, variance));
                
                GenerateReverseAverageRecursive(name, childBeforeTime, childTime, childValue, timeRange, aggregationLevels, nextAggregationIndex, maxAggregationLevelIndex, nextVariance, lowestAggregationValues);
            }
        }

        private long GetQuantisedTimeBeforeOrOn(int aggregationLevelSeconds, long timeSeconds)
        {
            return (long)Math.Floor((double)timeSeconds / aggregationLevelSeconds) * aggregationLevelSeconds;
        }

        private long GetQuantisedTimeAfterOrOn(int aggregationLevelSeconds, long timeSeconds)
        {
            return (long)Math.Ceiling((double)timeSeconds / aggregationLevelSeconds) * aggregationLevelSeconds;
        }

        private IEnumerable<long> GetQuantisedTimesBetweenOrAtEnd(int aggregationLevelSeconds, long lowTime, long highTime)
        {
            var quantisedLow = GetQuantisedTimeBeforeOrOn(aggregationLevelSeconds, lowTime);
            var quantisedHigh = GetQuantisedTimeAfterOrOn(aggregationLevelSeconds, highTime);

            for (var timeSeconds = quantisedLow; timeSeconds <= quantisedHigh; timeSeconds += aggregationLevelSeconds)
            {
                if (timeSeconds <= quantisedLow)
                {
                    continue;
                }

                yield return timeSeconds;
            }
        }

        private class LowestAggregationValues
        {
            private readonly long _minSeconds;
            private readonly int _lowestAggregationSeconds;
            private readonly double[] Values;

            public LowestAggregationValues(long minSeconds, long maxSeconds, int lowestAggregationSeconds)
            {
                _minSeconds = minSeconds;
                _lowestAggregationSeconds = lowestAggregationSeconds;
                var span = maxSeconds - minSeconds;
                Values = new double[span / lowestAggregationSeconds + 1];
            }

            public double GetOrAddValue(long timeSeconds, Func<double> valueGenerator)
            {
                var index = GetTimeIndex(timeSeconds);

                if (Values[index] == 0)
                {
                    Values[index] = valueGenerator();
                }

                return Values[index];
            }

            public void AddValue(long timeSeconds, double value)
            {
                var index = GetTimeIndex(timeSeconds);
                Values[index] = value;
            }

            public double GetValue(long timeSeconds)
            {
                var index = GetTimeIndex(timeSeconds);
                return Values[index];
            }

            public int GetTimeIndex(long timeSeconds)
            {
                return (int)(timeSeconds - _minSeconds) / _lowestAggregationSeconds;
            }
        }

        /*private class IndexProvider
        {
            private readonly long _minSeconds;
            private readonly int _lowestAggregationSeconds;

            public IndexProvider(long minSeconds, int lowestAggregationSeconds)
            {
                _minSeconds = minSeconds;
                _lowestAggregationSeconds = lowestAggregationSeconds;
            }

            public int GetIndex(long timeSeconds)
            {
                return (int) ((timeSeconds - _minSeconds) / _lowestAggregationSeconds);
            }
        }

        public class DataList : List<double?>
        {
            public double? Get
        }*/
    }
}
