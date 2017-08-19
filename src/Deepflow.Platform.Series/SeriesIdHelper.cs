using System;

namespace Deepflow.Platform.Series
{
    public static class SeriesIdHelper
    {
        public static string ToAttributeSeriesId(Guid entity, Guid attribute)
        {
            return $"{entity}:{attribute}";
        }

        public static string ToCalculationSeriesId(Guid entity, Guid calculation)
        {
            return $"{entity}:{calculation}";
        }
    }
}
