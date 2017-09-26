using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series
{
    public class DataValidator : IDataValidator
    {
        public void ValidateAtLeastOneDataRange(IEnumerable<AggregatedDataRange> dataRanges, string reason)
        {
            if (!dataRanges.Any())
            {
                throw new Exception(reason);
            }
        }

        public void ValidateExactlyOneDataRange(IEnumerable<AggregatedDataRange> dataRanges, string reason)
        {
            if (dataRanges.Count() != 1)
            {
                throw new Exception(reason);
            }
        }

        public void ValidateDataRangesIsOfAggregation(AggregatedDataRange dataRange, int aggregationSeconds, string reason)
        {
            if (dataRange.AggregationSeconds != aggregationSeconds)
            {
                throw new Exception(reason);
            }
        }

        public void ValidateExactlyOnePoint(AggregatedDataRange dataRange, string reason)
        {
            if (dataRange.Data.Count != 2)
            {
                throw new Exception(reason);
            }
        }

        public void ValidateDataRangesAreOfAggregation(IEnumerable<AggregatedDataRange> dataRanges, int aggregationSeconds, string reason)
        {
            foreach (var dataRange in dataRanges)
            {
                if (dataRange.AggregationSeconds != aggregationSeconds)
                {
                    throw new Exception(reason);
                }
            }
        }
    }
}
