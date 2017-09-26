using System.Collections.Generic;

namespace Deepflow.Platform.Abstractions.Series
{
    public interface IDataValidator
    {
        void ValidateAtLeastOneDataRange(IEnumerable<AggregatedDataRange> dataRanges, string reason);
        void ValidateExactlyOneDataRange(IEnumerable<AggregatedDataRange> dataRanges, string reason);
        void ValidateDataRangesIsOfAggregation(AggregatedDataRange dataRange, int aggregationSeconds, string reason);
        void ValidateExactlyOnePoint(AggregatedDataRange dataRange, string reason);
    }
}