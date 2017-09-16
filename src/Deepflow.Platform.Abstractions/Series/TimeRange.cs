using System;
using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public class TimeRange : IComparable<TimeRange>
    {
        public long MinSeconds { get; set; }
        public long MaxSeconds { get; set; }

        public TimeRange()
        {
            
        }
        
        public TimeRange(long minSeconds, long maxSeconds)
        {
            MinSeconds = minSeconds;
            MaxSeconds = maxSeconds;
        }

        public int CompareTo(TimeRange other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var minSecondsComparison = MinSeconds.CompareTo(other.MinSeconds);
            if (minSecondsComparison != 0) return minSecondsComparison;
            return MaxSeconds.CompareTo(other.MaxSeconds);
        }

        public static bool operator < (TimeRange one, TimeRange two)
        {
            return one.MinSeconds < two.MinSeconds;
        }
        
        public static bool operator > (TimeRange one, TimeRange two)
        {
            return one.MinSeconds > two.MinSeconds;
        }

        public TimeRange Insersect(TimeRange other)
        {
            if (MinSeconds > other.MaxSeconds)
            {
                return null;
            }

            if (MaxSeconds < other.MinSeconds)
            {
                return null;
            }

            if (other.MaxSeconds >= MaxSeconds && other.MinSeconds <= MinSeconds)
            {
                return new TimeRange(MinSeconds, MaxSeconds);
            }

            if (MinSeconds == other.MaxSeconds)
            {
                return new TimeRange(MinSeconds, MinSeconds);
            }

            if (MaxSeconds == other.MinSeconds)
            {
                return new TimeRange(MaxSeconds, MaxSeconds);
            }

            if (MinSeconds <= other.MinSeconds && MaxSeconds >= other.MaxSeconds)
            {
                return new TimeRange(other.MinSeconds, other.MaxSeconds);
            }

            if (MaxSeconds >= other.MinSeconds && MinSeconds < other.MinSeconds)
            {
                return new TimeRange(other.MinSeconds, MaxSeconds);
            }

            if (MaxSeconds >= other.MaxSeconds && MinSeconds < other.MaxSeconds)
            {
                return new TimeRange(MinSeconds, other.MaxSeconds);
            }

            if (MinSeconds > other.MinSeconds && MaxSeconds < other.MaxSeconds)
            {
                return new TimeRange(MinSeconds, MaxSeconds);
            }

            throw new Exception($"Can't intersect {this} with {other}");
        }

        public bool Insersects(long seconds)
        {
            if (seconds < MinSeconds)
            {
                return false;
            }

            if (seconds > MaxSeconds)
            {
                return false;
            }

            return true;
        }

        public bool IsZeroLength()
        {
            return MinSeconds == MaxSeconds;
        }

        public override string ToString()
        {
            return $"{MinSeconds}-{MaxSeconds}";
        }

        public TimeRange Quantise(int stepSeconds)
        {
            var minSeconds = (long) Math.Floor((double) MinSeconds / stepSeconds) * stepSeconds;
            var maxSeconds = (long) Math.Ceiling((double) MaxSeconds / stepSeconds) * stepSeconds;
            return new TimeRange(minSeconds, maxSeconds);
        }

        public bool EqualsMin(double timeSeconds)
        {
            return Math.Abs(timeSeconds - MinSeconds) < 0.001;
        }

        public IEnumerable<TimeRange> Chop(int spanSeconds)
        {
            if (spanSeconds <= 0)
            {
                throw new Exception("Can't chop zero length");
            }

            var minSeconds = MinSeconds;
            while (minSeconds < MaxSeconds)
            {
                var maxSeconds = minSeconds + spanSeconds;
                yield return new TimeRange(minSeconds, maxSeconds);
                minSeconds += spanSeconds;
            }
        }
    }
}
