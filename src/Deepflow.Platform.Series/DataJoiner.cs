using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;

namespace Deepflow.Platform.Series
{
    public class DataJoiner<TRange> : IDataJoiner<TRange> where TRange : IDataRange
    {
        private readonly IDataRangeCreator<TRange> _creator;

        public DataJoiner(IDataRangeCreator<TRange> creator)
        {
            _creator = creator;
        }

        public IEnumerable<TRange> JoinDataRangesToDataRanges(IEnumerable<TRange> dataRanges, IEnumerable<TRange> newDataRanges)
        {
            if (newDataRanges == null || !newDataRanges.Any())
            {
                return dataRanges;
            }

            if (dataRanges == null)
            {
                return newDataRanges;
            }

            IEnumerable<TRange> joined = dataRanges;

            foreach (var newDataRange in newDataRanges)
            {
                joined = JoinDataRangeToDataRanges(joined, newDataRange);
            }

            return joined;
        }

        public IEnumerable<TRange> JoinDataRangeToDataRanges(IEnumerable<TRange> ranges, TRange newRange)
        {
            if (ranges == null || !ranges.Any())
            {
                return new [] { newRange };
            }

            if (newRange == null)
            {
                return ranges;
            }
            
            return ranges.JoinInsert(newRange, PlaceDataRange, JoinTwoDataRanges);
        }

        private InsertPlacement PlaceDataRange(TRange itemToInsert, TRange existingItem)
        {
            if (itemToInsert.TimeRange.Touches(existingItem.TimeRange))
            {
                return InsertPlacement.Equal;
            }

            if (itemToInsert.TimeRange.MinSeconds < existingItem.TimeRange.MinSeconds)
            {
                return InsertPlacement.Before;
            }

            if (itemToInsert.TimeRange.MinSeconds > existingItem.TimeRange.MinSeconds)
            {
                return InsertPlacement.After;
            }

            throw new Exception("PlaceDataRange can't place data range");
        }

        private TRange JoinTwoDataRanges(TRange old, TRange insert)
        {
            var min = Math.Min(insert.TimeRange.MinSeconds, old.TimeRange.MinSeconds);
            var max = Math.Max(insert.TimeRange.MaxSeconds, old.TimeRange.MaxSeconds);

            var data = JoinData(insert.Data, old.Data);
            return _creator.Create(new TimeRange(min, max), data, insert);
        }

        private List<double> JoinData(List<double> insert, List<double> old)
        {
            if (insert == null || insert.Count == 0)
            {
                return old;
            }

            if (old == null || old.Count == 0)
            {
                return insert;
            }

            var endTakeOldIndex = BinarySearcher.GetFirstIndexPastTimestampBinary(old, insert[0]);
            var startTakeOldIndex = BinarySearcher.GetFirstIndexPastTimestampBinary(old, insert[insert.Count - 2]) + 2;

            return old.Take(endTakeOldIndex ?? old.Count).Concat(insert).Concat(old.Skip(startTakeOldIndex ?? old.Count)).ToList();
        }
    }
}