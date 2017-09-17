using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series
{
    public class DataMerger<TRange> : IDataMerger<TRange> where TRange : IDataRange
    {
        private readonly IDataFilterer<TRange> _filterer;
        private readonly IDataJoiner<TRange> _joiner;

        public DataMerger(IDataFilterer<TRange> filterer, IDataJoiner<TRange> joiner)
        {
            _filterer = filterer;
            _joiner = joiner;
        }

        public IEnumerable<TRange> MergeDataRangeWithRange(TRange range, TRange incomingRange)
        {
            return MergeDataRangeWithRanges(new List<TRange> { range }, incomingRange);
        }

        public IEnumerable<TRange> MergeDataRangesWithRanges(IEnumerable<TRange> ranges, IEnumerable<TRange> incomingRanges)
        {
            if (incomingRanges == null || !incomingRanges.Any())
            {
                return ranges;
            }

            IEnumerable<TRange> mergedRanges = new List<TRange>();
            foreach (var incomingRange in incomingRanges)
            {
                mergedRanges = MergeDataRangeWithRanges(mergedRanges, incomingRange);
            }
            return mergedRanges;
        }

        public IEnumerable<TRange> MergeDataRangeWithRanges(IEnumerable<TRange> ranges, TRange incomingRange)
        {
            return AddRangeToRanges(ranges, incomingRange);
        }

        private IEnumerable<TRange> AddRangeToRanges(IEnumerable<TRange> ranges, TRange incomingRange)
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
