using System;
using System.Collections.Generic;
using System.Linq;

namespace Deepflow.Platform.Abstractions.Series
{
    public class DataRange
    {
        public TimeRange TimeRange { get; set; }
        public List<double> Data { get; set; }

        private static double _maxAcceptableTime = (new DateTime(2050, 1, 1) - new DateTime(1970, 1, 1)).TotalSeconds;
        private static double _minAcceptableTime = 0;

        public DataRange() { }

        public DataRange(long minSeconds, long maxSeconds)
        {
            TimeRange = new TimeRange(minSeconds, maxSeconds);
        }

        public DataRange(long minSeconds, long maxSeconds, List<double> data)
        {
            TimeRange = new TimeRange(minSeconds, maxSeconds);
            Data = data;
        }

        public DataRange(TimeRange timeRange, List<double> data)
        {
            TimeRange = timeRange;
            Data = data;
        }

        public DataRange(TimeRange timeRange)
        {
            TimeRange = timeRange;
        }

        public DataRange(long minSeconds, long maxSeconds, IEnumerable<Datum> data)
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

        public bool Touches(DataRange otherRange)
        {
            if (otherRange == null)
            {
                return false;
            }

            if (otherRange.TimeRange.MaxSeconds >= TimeRange.MinSeconds && otherRange.TimeRange.MaxSeconds <= TimeRange.MaxSeconds)
            {
                return true;
            }

            if (otherRange.TimeRange.MinSeconds <= TimeRange.MaxSeconds && otherRange.TimeRange.MinSeconds >= TimeRange.MinSeconds)
            {
                return true;
            }

            if (otherRange.TimeRange.MinSeconds < TimeRange.MinSeconds && otherRange.TimeRange.MaxSeconds > TimeRange.MaxSeconds)
            {
                return true;
            }

            return false;
        }

        public IEnumerable<DataRange> Chop(int spanSeconds)
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
                    data.Add(Data[index * 2]);
                    data.Add(Data[index * 2 + 1]);
                    index++;
                }

                if (data.Any())
                {
                    yield return new DataRange(minSeconds, maxSeconds, data);
                }
                
                minSeconds += spanSeconds;
            }
        }

        public void Validate()
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
            
            for (var i = 0; i < numPoints; i++)
            {
                var time = Data[i * 2];
                if (time < _minAcceptableTime)
                {
                    throw new Exception($"Data range has time below minimum value of {_minAcceptableTime}");
                }
                if (time > _maxAcceptableTime)
                {
                    throw new Exception($"Data range has time above maximum value of {_maxAcceptableTime}");
                }

                var value = Data[i * 2 + 1];
                if (double.IsNaN(value))
                {
                    throw new Exception("Data range has NaN value");
                }
            }
        }
    }
}
