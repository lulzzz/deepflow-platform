using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataValidator
    {
        void ValidateAggregation(AggregatedDataRange dataRange, int aggregationLevel);
        bool IsValidDataRange(AggregatedDataRange dataRange);
        bool AreValidDataRanges(IEnumerable<AggregatedDataRange> dataRanges);
        void ValidateSingleOrLessDataRange(IEnumerable<AggregatedDataRange> dataRanges, string reason);
        void ValidateAtLeastOneDataRange(IEnumerable<AggregatedDataRange> dataRanges, string reason);
    }
}