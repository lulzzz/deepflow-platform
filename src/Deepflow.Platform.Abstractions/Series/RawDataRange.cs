using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series.Validators;
using FluentValidation;

namespace Deepflow.Platform.Abstractions.Series
{
    public class RawDataRange : IDataRange<RawDataRange, RawDataRangeCreator>
    {
        private static readonly RawDataRangeValidator Validator = new RawDataRangeValidator();
        public TimeRange TimeRange { get; set; }
        public List<double> Data { get; set; } = new List<double>();

        public RawDataRange() { }

        public RawDataRange(long minSeconds, long maxSeconds, bool skipValidation = false)
        {
            TimeRange = new TimeRange(minSeconds, maxSeconds);

            if (!skipValidation)
            {
                Validator.ValidateAndThrow(this);
            }
        }

        public RawDataRange(long minSeconds, long maxSeconds, List<double> data, bool skipValidation = false)
        {
            TimeRange = new TimeRange(minSeconds, maxSeconds);
            Data = data;

            if (!skipValidation)
            {
                Validator.ValidateAndThrow(this);
            }
        }

        public RawDataRange(TimeRange timeRange, List<double> data, bool skipValidation = false)
        {
            TimeRange = timeRange;
            Data = data;

            if (!skipValidation)
            {
                Validator.ValidateAndThrow(this);
            }
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

            Validator.ValidateAndThrow(this);
        }

        public IEnumerable<RawDataRange> Chop(int spanSeconds)
        {
            if (spanSeconds <= 0)
            {
                throw new Exception("Can't chop zero length");
            }

            var minSeconds = TimeRange.Min;
            var index = 0;
            var numPoints = Data.Count / 2;
            while (minSeconds < TimeRange.Max)
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
    }

    public class RawDataRangeCreator : IRangeCreator<RawDataRange>
    {
        public RawDataRange Create(TimeRange timeRange, List<double> data, RawDataRange previousRange)
        {
            return new RawDataRange(timeRange, data, true);
        }
    }

    public class RawDataRangeFilteringPolicy : IRangeFilteringPolicy<RawDataRange>
    {
        public FilterMode FilterMode { get; } = FilterMode.MinAndMaxInclusive;
        public bool AreZeroLengthRangesAllowed { get; } = true;
    }

    public class RawDataRangeAccessor : IRangeAccessor<RawDataRange>
    {
        public TimeRange GetTimeRange(RawDataRange range)
        {
            return range.TimeRange;
        }

        public List<double> GetData(RawDataRange range)
        {
            return range.Data;
        }
    }
}
