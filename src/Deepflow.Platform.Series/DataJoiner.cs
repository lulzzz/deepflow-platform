using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;

namespace Deepflow.Platform.Series
{
    public class DataJoiner : IDataJoiner
    {
        public IEnumerable<DataRange> JoinDataRangesToDataRanges(IEnumerable<DataRange> dataRanges, IEnumerable<DataRange> newDataRanges)
        {
            if (newDataRanges == null || !newDataRanges.Any())
            {
                return dataRanges;
            }

            if (dataRanges == null)
            {
                return newDataRanges;
            }

            IEnumerable<DataRange> joined = dataRanges;

            foreach (var newDataRange in newDataRanges)
            {
                joined = JoinDataRangeToDataRanges(joined, newDataRange);
            }

            return joined;
        }

        public IEnumerable<DataRange> JoinDataRangeToDataRanges(IEnumerable<DataRange> ranges, DataRange newRange)
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

        private InsertPlacement PlaceDataRange(DataRange itemToInsert, DataRange existingItem)
        {
            if (itemToInsert.Touches(existingItem))
            {
                return InsertPlacement.Equal;
            }

            if (itemToInsert.TimeRange < existingItem.TimeRange)
            {
                return InsertPlacement.Before;
            }

            if (itemToInsert.TimeRange > existingItem.TimeRange)
            {
                return InsertPlacement.After;
            }

            throw new Exception("PlaceDataRange can't place data range");
        }

        private DataRange JoinTwoDataRanges(DataRange old, DataRange insert)
        {
            var min = Math.Min(insert.TimeRange.MinSeconds, old.TimeRange.MinSeconds);
            var max = Math.Max(insert.TimeRange.MaxSeconds, old.TimeRange.MaxSeconds);

            var data = JoinData(insert.Data, old.Data);
            return new DataRange(min, max, data);
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