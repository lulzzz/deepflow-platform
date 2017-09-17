using System;
using System.Collections.Generic;
using System.Linq;

namespace Deepflow.Platform.Abstractions.Series
{
    public class RawDataRange : IDataRange<RawDataRange, RawDataRangeCreator>
    {
        public TimeRange TimeRange { get; set; }
        public List<double> Data { get; set; }

        private static readonly double MaxAcceptableTime = (new DateTime(2100, 1, 1) - new DateTime(1970, 1, 1)).TotalSeconds;
        private static readonly double MinAcceptableTime = 0;

        public RawDataRange() { }

        public RawDataRange(long minSeconds, long maxSeconds)
        {
            TimeRange = new TimeRange(minSeconds, maxSeconds);
        }

        public RawDataRange(long minSeconds, long maxSeconds, List<double> data)
        {
            TimeRange = new TimeRange(minSeconds, maxSeconds);
            Data = data;
        }

        public RawDataRange(TimeRange timeRange, List<double> data)
        {
            TimeRange = timeRange;
            Data = data;
        }

        public RawDataRange(long minSeconds, long maxSeconds, IEnumerable<Datum> data)
        {
            TimeRange = new TimeRange(minSeconds, maxSeconds);

            var dataArray = new double[data.Count() * 2];
            var i = 0;
            foreach (var datum in data)
            {
                dataArray[i * 2] = datum.Time;
                dataArray[i * 2 + 1] = datum.Value;
                i++;
            }

            Data = dataArray.ToList();
        }

        public IEnumerable<RawDataRange> Chop(int spanSeconds)
        {
            if (spanSeconds <= 0)
            {
                throw new Exception("Can't chop zero length");
            }

            var minSeconds = TimeRange.MinSeconds;
            var index = 0;
            var numPoints = Data.Count / 2;
            while (minSeconds < TimeRange.MaxSeconds)
            {
                var maxSeconds = minSeconds + spanSeconds;

                while (index < numPoints && Data[index * 2] <= minSeconds)
                {
                    index++;
                }

                var data = new List<double>();
                while (index < numPoints && Data[index * 2] <= maxSeconds)
                {
                    var time = Data[index * 2];
                    var value = Data[index * 2 + 1];
                    data.Add(time);
                    data.Add(value);
                    index++;
                }

                if (data.Any())
                {
                    yield return new RawDataRange(minSeconds, maxSeconds, data);
                }
                
                minSeconds += spanSeconds;
            }
        }

        public void Validate()
        {
            if (TimeRange == null)
            {
                throw new Exception("Data range does not have a time range");
            }

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

            // Aggregation seconds zero or above

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
                if (time < minTime)
                {
                    throw new Exception($"Data range has data point time below minimum value of {MinAcceptableTime}");
                }

                    var value = Data[i * 2 + 1];
                if (double.IsNaN(value))
                {
                    throw new Exception("Data range has NaN value");
                }
            }
        }
    }

    public class RawDataRangeCreator : IDataRangeCreator<RawDataRange>
    {
        public RawDataRange Create(TimeRange timeRange, List<double> data, RawDataRange previousRange)
        {
            return new RawDataRange(timeRange, data);
        }
    }
}
