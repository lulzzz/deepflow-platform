﻿using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public class DataRange
    {
        public TimeRange TimeRange { get; set; }
        public List<double> Data { get; set; }

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
    }
}