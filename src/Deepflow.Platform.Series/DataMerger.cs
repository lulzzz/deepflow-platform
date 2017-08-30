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

        public IEnumerable<DataRange> MergeDataRangeWithRange(DataRange range, DataRange incomingRange)
        {
            return MergeDataRangeWithRanges(new List<DataRange> { range }, incomingRange);
        }

        public IEnumerable<DataRange> MergeDataRangesWithRanges(IEnumerable<DataRange> ranges, IEnumerable<DataRange> incomingRanges)
        {
            IEnumerable<DataRange> mergedRanges = new List<DataRange>();
            foreach (var incomingRange in incomingRanges)
            {
                mergedRanges = MergeDataRangeWithRanges(mergedRanges, incomingRange);
            }
            return mergedRanges;
        }

        public IEnumerable<DataRange> MergeDataRangeWithRanges(IEnumerable<DataRange> ranges, DataRange incomingRange)
        {
            return AddRangeToRanges(ranges, incomingRange);
        }

        private IEnumerable<DataRange> AddRangeToRanges(IEnumerable<DataRange> ranges, DataRange incomingRange)
        {
            if (incomingRange == null)
            {
                return ranges;
            }

            var subtractedRanges = _filterer.SubtractTimeRangeFromRanges(ranges, incomingRange.TimeRange);
            return _joiner.JoinDataRangeToDataRanges(subtractedRanges, incomingRange);
        }
    }
}
