using System;

namespace Deepflow.Platform.Core.Tools
{
    public static class DateTimeExtensions
    {
        public static long SecondsSince1970Utc(this DateTime utcDateTime)
        {
            return (long) utcDateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static DateTime ToDateTime(this long utcSeconds)
        {
            return new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(utcSeconds);
        }
    }
}
