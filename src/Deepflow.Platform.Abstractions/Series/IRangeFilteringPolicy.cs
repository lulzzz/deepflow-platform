using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IRangeFilteringPolicy<TDataRange>
    {
        FilterMode FilterMode { get; }
        bool AreZeroLengthRangesAllowed { get; }
    }
}