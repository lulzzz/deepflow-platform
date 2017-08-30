using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataValidator
    {
        void ValidateAggregation(DataRange dataRange, int aggregationLevel);
        bool IsValidDataRange(DataRange dataRange);
        bool AreValidDataRanges(IEnumerable<DataRange> dataRanges);
        void ValidateSingleOrLessDataRange(IEnumerable<DataRange> dataRanges, string reason);
        void ValidateAtLeastOneDataRange(IEnumerable<DataRange> dataRanges, string reason);
    }
}