using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;

namespace Deepflow.Platform.Series
{
    public class DataValidator : IDataValidator
    {
        public void ValidateAggregation(DataRange dataRange, int aggregationLevel)
        {
            foreach (var datum in dataRange.GetData())
            {
                if (datum.Time % aggregationLevel != 0)
                {
                    throw new Exception($"Data does not fit aggregation level of '{aggregationLevel}'");
                }
            }
        }

        public bool IsValidDataRange(DataRange dataRange)
        {
            if (dataRange == null || dataRange.TimeRange.IsZeroLength() || dataRange.Data == null || dataRange.Data.Count == 0)
            {
                return false;
            }

            return true;
        }

        public bool AreValidDataRanges(IEnumerable<DataRange> dataRanges)
        {
            return dataRanges != null && dataRanges.Any();
        }

        public void ValidateSingleOrLessDataRange(IEnumerable<DataRange> dataRanges, string reason)
        {
            if (dataRanges.Count() >= 2)
            {
                throw new Exception(reason);
            }
        }

        public void ValidateAtLeastOneDataRange(IEnumerable<DataRange> dataRanges, string reason)
        {
            if (!dataRanges.Any())
            {
                throw new Exception(reason);
            }
        }
    }
}
