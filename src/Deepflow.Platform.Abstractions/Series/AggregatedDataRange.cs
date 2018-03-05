using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deepflow.Platform.Abstractions.Series.Validators;
using FluentValidation;
using Newtonsoft.Json;

namespace Deepflow.Platform.Abstractions.Series
{
    public class AggregatedDataRange : IDataRange<AggregatedDataRange, AggregatedRangeCreator>
    {
        private static readonly AggregatedDataRangeValidator Validator = new AggregatedDataRangeValidator();
        public TimeRange TimeRange { get; set; }
        public List<double> Data { get; set; }
        public int AggregationSeconds { get; set; }

        public AggregatedDataRange() { }

        public AggregatedDataRange(long minSeconds, long maxSeconds, List<double> data, int aggregationSeconds, bool skipValidation = false)
        {
            TimeRange = new TimeRange(minSeconds, maxSeconds);
            AggregationSeconds = aggregationSeconds;
            Data = data;
            
            if (!skipValidation)
            {
                try
                {
                    Validator.ValidateAndThrow(this);
                }
                catch (Exception)
                {
                    File.WriteAllText("ValidationFailed.json", JsonConvert.SerializeObject(data));
                    throw;
                }
            }
        }

        public AggregatedDataRange(TimeRange timeRange, List<double> data, int aggregationSeconds, bool skipValidation = false)
        {
            TimeRange = timeRange;
            AggregationSeconds = aggregationSeconds;
            Data = data;
            
            if (!skipValidation)
            {
                Validator.ValidateAndThrow(this);
            }
        }

        public IEnumerable<AggregatedDataRange> Chop(int spanSeconds)
        {
            if (spanSeconds <= 0)
            {
                throw new Exception("Can't chop zero length");
            }

            var quantisedSpanSeconds = spanSeconds - (spanSeconds % AggregationSeconds) + AggregationSeconds;

            var minSeconds = TimeRange.Min;
            var index = 0;
            var numPoints = Data.Count / 2;
            while (minSeconds < TimeRange.Max)
            {
                var maxSeconds = minSeconds + quantisedSpanSeconds;

                while (index < numPoints && Data[index * 2] <= minSeconds)
                {
                    index++;
                }

                var data = new List<double>();
                var minTime = double.MaxValue;
                var maxTime = double.MinValue;
                while (index < numPoints && Data[index * 2] <= maxSeconds)
                {
                    var time = Data[index * 2];
                    var value = Data[index * 2 + 1];
                    data.Add(time);
                    data.Add(value);
                    minTime = Math.Min(minTime, time);
                    maxTime = Math.Min(maxTime, time);
                    index++;
                }

                if (data.Any())
                {
                    var sliceMinTime = (long)minTime - AggregationSeconds;
                    var sliceMaxTime = (long)maxTime;
                    yield return new AggregatedDataRange(sliceMinTime, sliceMaxTime, data, AggregationSeconds);
                }

                minSeconds += quantisedSpanSeconds;
            }
        }
    }

    public class AggregatedRangeCreator : IRangeCreator<AggregatedDataRange>
    {
        public AggregatedDataRange Create(TimeRange timeRange, List<double> data, AggregatedDataRange range)
        {
            if (data.Any())
            {
                var minTime = data[0];
                var quantisedMinTime = minTime - range.AggregationSeconds;
                var newMinTime = (long)Math.Min(quantisedMinTime, timeRange.Min);
                var quantisedTimeRange = new TimeRange(newMinTime, timeRange.Max);
                return new AggregatedDataRange(quantisedTimeRange, data, range.AggregationSeconds, true);
            }
            return new AggregatedDataRange(timeRange, data, range.AggregationSeconds);
        }
    }

    public class AggregateRangeFilteringPolicy : IRangeFilteringPolicy<AggregatedDataRange>
    {
        public FilterMode FilterMode { get; } = FilterMode.MaxInclusive;
        public bool AreZeroLengthRangesAllowed { get; } = false;
    }

    public class AggregatedRangeAccessor : IRangeAccessor<AggregatedDataRange>
    {
        public TimeRange GetTimeRange(AggregatedDataRange range)
        {
            return range.TimeRange;
        }

        public List<double> GetData(AggregatedDataRange range)
        {
            return range.Data;
        }
    }
}
