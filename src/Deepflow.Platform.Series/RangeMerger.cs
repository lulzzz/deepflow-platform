using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series
{
    public class RangeMerger<TRange> : IRangeMerger<TRange>
    {
        private readonly IRangeFilterer<TRange> _filterer;
        private readonly IRangeJoiner<TRange> _joiner;
        private readonly IRangeAccessor<TRange> _accessor;

        public RangeMerger(IRangeFilterer<TRange> filterer, IRangeJoiner<TRange> joiner, IRangeAccessor<TRange> accessor)
        {
            _filterer = filterer;
            _joiner = joiner;
            _accessor = accessor;
        }

        public IEnumerable<TRange> MergeRangeWithRange(TRange range, TRange incomingRange)
        {
            return MergeRangeWithRanges(new List<TRange> { range }, incomingRange);
        }

        public IEnumerable<TRange> MergeRangesWithRange(TRange range, IEnumerable<TRange> incomingRanges)
        {
            return MergeRangesWithRanges(new [] { range }, incomingRanges);
        }

        public IEnumerable<TRange> MergeRangesWithRanges(IEnumerable<TRange> ranges, IEnumerable<TRange> incomingRanges)
        {
            if (incomingRanges == null || !incomingRanges.Any())
            {
                return ranges;
            }

            IEnumerable<TRange> mergedRanges = ranges;
            foreach (var incomingRange in incomingRanges) //.OrderBy(x => _accessor.GetTimeRange(x).Min)
            {
                mergedRanges = MergeRangeWithRanges(mergedRanges, incomingRange);
            }
            return mergedRanges;
        }

        public IEnumerable<TRange> MergeRangeWithRanges(IEnumerable<TRange> ranges, TRange incomingRange)
        {
            return AddRangeToRanges(ranges, incomingRange);
        }

        private IEnumerable<TRange> AddRangeToRanges(IEnumerable<TRange> ranges, TRange incomingRange)
        {
            if (incomingRange == null)
            {
                return ranges;
            }

            var subtractedRanges = _filterer.SubtractTimeRangeFromRanges(ranges, _accessor.GetTimeRange(incomingRange));
            return _joiner.JoinDataRangeToDataRanges(subtractedRanges, incomingRange);
        }
    }
}
