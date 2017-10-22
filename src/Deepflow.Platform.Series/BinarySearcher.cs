using System;
using System.Collections.Generic;

namespace Deepflow.Platform.Series
{
    public static class BinarySearcher
    {
        public static int? GetFirstIndexEqualOrGreaterTimestampBinary(List<double> data, double timestamp) {
            return GetIndexRecursive(data, timestamp, 0, data.Count - 2);
        }

        public static int? GetIndexRecursive(List<double> data, double timestamp, int min, int max)
        {
            if (data == null || data.Count == 0)
            {
                return null;
            }

            if (min == max)
            {
                return timestamp <= data[min] ? 0 : (int?) null;
            }

            var minTime = data[min];
            var maxTime = data[max];

            if (timestamp <= minTime)
            {
                return min;
            }

            if (timestamp > maxTime)
            {
                return null;
            }

            if (min == max - 2)
            {
                return timestamp <= maxTime ? max : (int?)null;
            }

            var mid = (int) Math.Floor((double)(min / 2 + max / 2) / 2) * 2;

            var lowerResult = GetIndexRecursive(data, timestamp, min, mid);
            if (lowerResult != null)
            {
                return lowerResult;
            }
            return GetIndexRecursive(data, timestamp, mid, max);
        }
}
}
