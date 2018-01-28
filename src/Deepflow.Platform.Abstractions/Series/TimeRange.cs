using System;
using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series.Validators;
using Deepflow.Platform.Core.Tools;
using FluentValidation;

namespace Deepflow.Platform.Abstractions.Series
{
    public class TimeRange : IComparable<TimeRange>
    {
        private static TimeRangeValidator _validator = new TimeRangeValidator();

        /// <summary>
        /// UTC seconds since Jan 1, 1970
        /// </summary>
        public long Min { get; set; }

        /// <summary>
        /// UTC seconds since Jan 1, 1970
        /// </summary>
        public long Max { get; set; }

        public TimeRange()
        {
            
        }
        
        public TimeRange(long min, long max)
        {
            Min = min;
            Max = max;

            _validator.ValidateAndThrow(this);
        }

        public TimeRange(DateTime min, DateTime max)
        {
            Min = min.SecondsSince1970Utc();
            Max = max.SecondsSince1970Utc();

            _validator.ValidateAndThrow(this);
        }

        public int CompareTo(TimeRange other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var minSecondsComparison = Min.CompareTo(other.Min);
            if (minSecondsComparison != 0) return minSecondsComparison;
            return Max.CompareTo(other.Max);
        }

        /*public static bool operator < (TimeRange one, TimeRange two)
        {
            return one.Min < two.Min;
        }
        
        public static bool operator > (TimeRange one, TimeRange two)
        {
            return one.Min > two.Min;
        }*/

        public void Validate()
        {
            _validator.ValidateAndThrow(this);
        }

        public TimeRange Insersect(TimeRange other)
        {
            if (Min > other.Max)
            {
                return null;
            }

            if (Max < other.Min)
            {
                return null;
            }

            if (other.Max >= Max && other.Min <= Min)
            {
                return new TimeRange(Min, Max);
            }

            if (Min == other.Max)
            {
                return new TimeRange(Min, Min);
            }

            if (Max == other.Min)
            {
                return new TimeRange(Max, Max);
            }

            if (Min <= other.Min && Max >= other.Max)
            {
                return new TimeRange(other.Min, other.Max);
            }

            if (Max >= other.Min && Min < other.Min)
            {
                return new TimeRange(other.Min, Max);
            }

            if (Max >= other.Max && Min < other.Max)
            {
                return new TimeRange(Min, other.Max);
            }

            if (Min > other.Min && Max < other.Max)
            {
                return new TimeRange(Min, Max);
            }

            throw new Exception($"Can't intersect {this} with {other}");
        }

        public bool Insersects(long seconds)
        {
            if (seconds < Min)
            {
                return false;
            }

            if (seconds > Max)
            {
                return false;
            }

            return true;
        }

        public bool Touches(TimeRange otherRange)
        {
            if (otherRange == null)
            {
                return false;
            }

            if (otherRange.Max >= Min && otherRange.Max <= Max)
            {
                return true;
            }

            if (otherRange.Min <= Max && otherRange.Min >= Min)
            {
                return true;
            }

            if (otherRange.Min < Min && otherRange.Max > Max)
            {
                return true;
            }

            return false;
        }

        public bool IsZeroLength()
        {
            return Min == Max;
        }

        public override string ToString()
        {
            return $"{Min}-{Max}";
        }

        public TimeRange Quantise(int stepSeconds)
        {
            var minSeconds = (long) Math.Floor((double) Min / stepSeconds) * stepSeconds;
            var maxSeconds = (long) Math.Ceiling((double) Max / stepSeconds) * stepSeconds;
            return new TimeRange(minSeconds, maxSeconds);
        }
        
        public bool IsQuantisedTo(int aggregationSeconds)
        {
            var quantised = Quantise(aggregationSeconds);
            return quantised.Min == Min && quantised.Max == Max;
        }

        public bool EqualsMin(double timeSeconds)
        {
            return Math.Abs(timeSeconds - Min) < 0.001;
        }

        public IEnumerable<TimeRange> Chop(int spanSeconds)
        {
            if (spanSeconds <= 0)
            {
                throw new Exception("Can't chop zero length");
            }

            if (Min == Max)
            {
                yield return this;
                yield break;
            }

            var minSeconds = Min;
            while (minSeconds < Max)
            {
                var maxSeconds = minSeconds + spanSeconds;
                yield return new TimeRange(minSeconds, maxSeconds);
                minSeconds += spanSeconds;
            }
        }

        public override int GetHashCode()
        {
            return (int) (Min ^ Max);
        }

        public override bool Equals(object obj)
        {
            var other = obj as TimeRange;
            if (other == null)
            {
                return false;
            }
            return other.Min == Min && other.Max == Max;
        }
    }

    public class TimeRangeCreator : IRangeCreator<TimeRange>
    {
        public TimeRange Create(TimeRange timeRange, List<double> data, TimeRange previousRange)
        {
            return timeRange;
        }
    }

    public class TimeRangeFilteringPolicy : IRangeFilteringPolicy<TimeRange>
    {
        public FilterMode FilterMode { get; } = FilterMode.MinAndMaxInclusive;
        public bool AreZeroLengthRangesAllowed { get; } = true;
    }

    public class TimeRangeAccessor : IRangeAccessor<TimeRange>
    {
        public TimeRange GetTimeRange(TimeRange range)
        {
            return range;
        }

        public List<double> GetData(TimeRange range)
        {
            return null;
        }
    }
}
