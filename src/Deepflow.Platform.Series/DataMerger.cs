using System.Collections.Generic;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series
{
    public class DataMerger : IDataMerger
    {
        private readonly IDataFilterer _filterer;
        private readonly IDataJoiner _joiner;

        public DataMerger(IDataFilterer filterer, IDataJoiner joiner)
        {
            _filterer = filterer;
            _joiner = joiner;
        }

        public IEnumerable<DataRange> MergeDataRangesWithRanges(IEnumerable<DataRange> ranges, IEnumerable<DataRange> dataRanges)
        {
            IEnumerable<DataRange> mergedRanges = new List<DataRange>();
            foreach (var dataRange in dataRanges)
            {
                mergedRanges = MergeDataRangeWithRanges(mergedRanges, dataRange);
            }
            return mergedRanges;
        }

        public IEnumerable<DataRange> MergeDataRangeWithRanges(IEnumerable<DataRange> ranges, DataRange dataRange)
        {
            return AddRangeToRanges(ranges, dataRange);
        }

        private IEnumerable<DataRange> AddRangeToRanges(IEnumerable<DataRange> ranges, DataRange newRange)
        {
            if (newRange == null)
            {
                return ranges;
            }

            var subtractedRanges = _filterer.SubtractTimeRangeFromRanges(ranges, newRange.TimeRange);
            return _joiner.JoinDataRangeToDataRanges(subtractedRanges, newRange);
        }
    }
}
