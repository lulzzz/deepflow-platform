using System;

namespace Deepflow.Platform.Abstractions.Series
{
    public class TimeRange : IComparable<TimeRange>
    {
        public long MinSeconds { get; set; }
        public long MaxSeconds { get; set; }

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
    }
}
