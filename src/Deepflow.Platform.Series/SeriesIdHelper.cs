using System;

namespace Deepflow.Platform.Series
{
    public static class SeriesIdHelper
    {
        public static string ToSeriesId(Guid entity, Guid attribute)
        {
            return $"{entity}:{attribute}";
        }
    }
}
