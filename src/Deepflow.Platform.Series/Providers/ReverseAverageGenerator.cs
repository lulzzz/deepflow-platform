/*
using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series.Providers
{
    public class ReverseAverageGenerator
    {
        private readonly IRangeFilterer _dataFilterer;
        private readonly IValueGenerator _valueGenerator;
        private readonly IRangeMerger _dataMerger;

        public ReverseAverageGenerator(IRangeFilterer dataFilterer, IValueGenerator valueGenerator, IRangeMerger dataMerger)
        {
            _dataFilterer = dataFilterer;
            _valueGenerator = valueGenerator;
            _dataMerger = dataMerger;
        }

        public RawDataRange GenerateReverseAverage(string name, TimeRange timeRange, int[] aggregationLevels, int aggregationLevel, double minValue, double maxValue)
        {
            // Get quantised time ranges
            var ascendingAggregationLevels = aggregationLevels.OrderByDescending(x => x).ToArray();
            var highestAggregationLevel = aggregationLevels.Max();
            var aggregationLevelIndex = Array.IndexOf(ascendingAggregationLevels, aggregationLevel);
            var quantisedLow = GetQuantisedTimeBeforeOrOn(highestAggregationLevel, timeRange.Min);
            var quantisedHigh = GetQuantisedTimeAfterOrOn(highestAggregationLevel, timeRange.Max);
            IEnumerable<RawDataRange> dataRanges = new List<RawDataRange>();

            // For each quantised time range
            for (var time = quantisedLow; time < quantisedHigh; time += highestAggregationLevel)
            {
                var lowTime = time;
                var highTime = time + highestAggregationLevel;

                var rangeValues = GenerateReverseAverageForRange(name, lowTime, highTime, timeRange, ascendingAggregationLevels, aggregationLevelIndex, minValue, maxValue);
                var levelValues = rangeValues[aggregationLevel];
                List<double> levelData = new List<double>();
                for (var i = 0; i < levelValues.Length; i++)
                {
                    if (levelValues[i] == null)
                    {
                        continue;
                    }
                    levelData.Add(lowTime + i * aggregationLevel);
                    levelData.Add(levelValues[i].Value);
                }
                var dataRange = new RawDataRange(lowTime, highTime, new List<double>(levelData));
                dataRanges = _dataMerger.MergeRangeWithRanges(dataRanges, dataRange);
            }
            
            return _dataFilterer.FilterDataRange(dataRanges.SingleOrDefault(), timeRange);
        }

        private RangeValues GenerateReverseAverageForRange(string name, long lowTime, long highTime, TimeRange timeRange, int[] aggregationLevels, int maxAggregationLevelIndex, double minValue, double maxValue)
        {
            var rangeValues = new RangeValues(new TimeRange(lowTime, highTime));

            // Generate min and max value
            var lowValue = _valueGenerator.GenerateValue(name, aggregationLevels[0], lowTime, minValue, maxValue);
            var highValue = _valueGenerator.GenerateValue(name, aggregationLevels[0], highTime, minValue, maxValue);

            // Add to data array
            rangeValues.GetOrAddLevelValue(aggregationLevels[0], 0, () => lowValue);
            rangeValues.GetOrAddLevelValue(aggregationLevels[0], 1, () => highValue);

            // Generate child points
            GenerateReverseAverageRecursive(name, lowTime, highTime, lowValue, highValue, timeRange, aggregationLevels, 1, maxAggregationLevelIndex, rangeValues);

            return rangeValues;
        }

        private void GenerateReverseAverageRecursive(string name, long lowTime, long highTime, double lowValue, double highValue, TimeRange timeRange, int[] aggregationLevels, int aggregationLevelIndex, int maxAggregationLevelIndex, RangeValues rangeValues)
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
            var childTimes = GetQuantisedTimesBetweenOrAtEnd(aggregationLevel, lowTime, highTime);
            foreach (var childTime in childTimes)
            {
                GenerateReverseAverageTimeRecursive(name, lowValue, highValue, childTime, timeRange, aggregationLevels, aggregationLevelIndex, maxAggregationLevelIndex, rangeValues);
            }
        }

        private void GenerateReverseAverageTimeRecursive(string name, double parentLowValue, double parentHighValue, long timeSeconds, TimeRange timeRange, int[] aggregationLevels, int aggregationLevelIndex, int maxAggregationLevelIndex, RangeValues rangeValues)
        {
            var aggregationLevel = aggregationLevels[aggregationLevelIndex];

            var lowTime = timeSeconds - aggregationLevel;
            var lowIndex = rangeValues.GetTimeIndex(aggregationLevel, lowTime);
            var lowValue = rangeValues.GetOrAddLevelValue(aggregationLevel, lowIndex, () => _valueGenerator.GenerateValue(name, aggregationLevel, lowTime, parentLowValue, parentHighValue));

            var highTime = timeSeconds;
            var highIndex = rangeValues.GetTimeIndex(aggregationLevel, highTime);
            var highValue = rangeValues.GetOrAddLevelValue(aggregationLevel, highIndex, () => _valueGenerator.GenerateValue(name, aggregationLevel, highTime, parentLowValue, parentHighValue));

            GenerateReverseAverageRecursive(name, lowTime, highTime, lowValue, highValue, timeRange, aggregationLevels, aggregationLevelIndex + 1, maxAggregationLevelIndex, rangeValues);
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

        private class RangeValues : Dictionary<int, double?[]>
        {
            private readonly TimeRange _timeRange;
            private readonly long _spanSeconds;

            public RangeValues(TimeRange timeRange)
            {
                _timeRange = timeRange;
                _spanSeconds = timeRange.Max - timeRange.Min;
            }

            public double GetOrAddLevelValue(int aggregationLevelSeconds, int index, Func<double> valueGenerator)
            {
                if (!TryGetValue(aggregationLevelSeconds, out double?[] levelData))
                {
                    levelData = new double?[_spanSeconds / aggregationLevelSeconds + 1];
                    Add(aggregationLevelSeconds, levelData);
                }

                if (levelData[index] == null)
                {
                    levelData[index] = valueGenerator();
                }

                return levelData[index].Value;
            }

            public int GetTimeIndex(int aggregationLevelSeconds, long timeSeconds)
            {
                return (int)(timeSeconds - _timeRange.Min) / aggregationLevelSeconds;
            }
        }
    }
}
*/
