using System;
using System.Collections.Generic;
using System.Linq;

namespace Deepflow.Platform.Abstractions.Series
{
    public class AggregatedDataRange : IDataRange<AggregatedDataRange, AggregatedDataRangeCreator>
    {
        public TimeRange TimeRange { get; set; }
        public List<double> Data { get; set; }
        public int AggregationSeconds { get; set; }

        private static readonly double MaxAcceptableTime = (new DateTime(2100, 1, 1) - new DateTime(1970, 1, 1)).TotalSeconds;
        private static readonly double MinAcceptableTime = 0;

        public AggregatedDataRange() { }

        public AggregatedDataRange(long minSeconds, long maxSeconds, int aggregationSeconds)
        {
            AggregationSeconds = aggregationSeconds;
            TimeRange = new TimeRange(minSeconds, maxSeconds);

            ValidateMinAndMaxSeconds();
            ValidateAggregationSeconds();
        }

        public AggregatedDataRange(long minSeconds, long maxSeconds, List<double> data, int aggregationSeconds)
        {
            TimeRange = new TimeRange(minSeconds, maxSeconds);
            AggregationSeconds = aggregationSeconds;
            Data = data;

            ValidateMinAndMaxSeconds();
            ValidateAggregationSeconds();
        }

        public AggregatedDataRange(TimeRange timeRange, List<double> data, int aggregationSeconds)
        {
            TimeRange = timeRange;
            AggregationSeconds = aggregationSeconds;
            Data = data;

            ValidateMinAndMaxSeconds();
            ValidateAggregationSeconds();
        }

        public IEnumerable<AggregatedDataRange> Chop(int spanSeconds)
        {
            if (spanSeconds <= 0)
            {
                throw new Exception("Can't chop zero length");
            }

            var quantisedSpanSeconds = spanSeconds - (spanSeconds % AggregationSeconds) + AggregationSeconds;

            var minSeconds = TimeRange.MinSeconds;
            var index = 0;
            var numPoints = Data.Count / 2;
            while (minSeconds < TimeRange.MaxSeconds)
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

        public void Validate()
        {
            if (TimeRange == null)
            {
                throw new Exception("Data range does not have a time range");
            }

            ValidateMinAndMaxSeconds();
            ValidateAggregationSeconds();
            ValidateData();
        }

        private void ValidateMinAndMaxSeconds()
        {
            if (TimeRange.MinSeconds < MinAcceptableTime)
            {
                throw new Exception($"Data range has min time of {TimeRange.MinSeconds} below minimum value of {MinAcceptableTime}");
            }

            if (TimeRange.MaxSeconds > MaxAcceptableTime)
            {
                throw new Exception($"Data range has max time of {TimeRange.MaxSeconds} above maximum value of {MaxAcceptableTime}");
            }

            if (TimeRange.MaxSeconds < TimeRange.MinSeconds)
            {
                throw new Exception($"Data range has max time of {TimeRange.MaxSeconds} below min time of {TimeRange.MinSeconds}");
            }
        }

        private void ValidateAggregationSeconds()
        {
            if (AggregationSeconds == 0)
            {
                throw new Exception("Aggregated data range cannot have an aggregation seconds of 0");
            }
        }

        private void ValidateData()
        {
            if (Data == null)
            {
                throw new Exception("Data range does not have data");
            }

            if (Data.Count % 2 != 0)
            {
                throw new Exception("Data range has odd number of values");
            }

            var numPoints = Data.Count / 2;

            if (numPoints == 0)
            {
                return;
            }

            var minTime = TimeRange.MinSeconds;
            var maxTime = TimeRange.MaxSeconds;
            var lastTime = -1.0;

            for (var i = 0; i < numPoints; i++)
            {
                var time = Data[i * 2];
                if (time < MinAcceptableTime)
                {
                    throw new Exception($"Data range has data point time below minimum value of {MinAcceptableTime}");
                }
                if (time > MaxAcceptableTime)
                {
                    throw new Exception($"Data range has data point time above maximum value of {MaxAcceptableTime}");
                }
                if (time <= minTime)
                {
                    throw new Exception($"Data range has data point time of {time} below or equal to the min seconds of {minTime}");
                }
                if (time > maxTime)
                {
                    throw new Exception($"Data range has data point time of {time} above the max seconds of {maxTime}");
                }
                if (Math.Abs(time - lastTime) < 0.5)
                {
                    throw new Exception($"Data range has duplicate points at time of {time}");
                }
                if (time < lastTime)
                {
                    throw new Exception($"Data range has an out of order point at time {time} which is before {lastTime}");
                }
                if (Math.Abs(time % AggregationSeconds) > 0.5)
                {
                    throw new Exception($"Data range has a point at time {time} which is not quantised to {AggregationSeconds} seconds");
                }
                
                var value = Data[i * 2 + 1];
                if (double.IsNaN(value))
                {
                    throw new Exception("Data range has NaN value");
                }
            }
        }
    }

    public class AggregatedDataRangeCreator : IDataRangeCreator<AggregatedDataRange>
    {
        public AggregatedDataRange Create(TimeRange timeRange, List<double> data, AggregatedDataRange range)
        {
            return new AggregatedDataRange(timeRange, data, range.AggregationSeconds);
        }
    }
}
