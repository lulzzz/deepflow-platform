using System;
using System.Collections.Generic;
using System.Linq;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Series
{
    public class RangeJoiner<TRange> : IRangeJoiner<TRange>
    {
        private readonly IRangeCreator<TRange> _creator;
        private readonly IRangeAccessor<TRange> _accessor;
        private readonly ILogger<RangeJoiner<TRange>> _logger;

        public RangeJoiner(IRangeCreator<TRange> creator, IRangeAccessor<TRange> accessor, ILogger<RangeJoiner<TRange>> logger)
        {
            _creator = creator;
            _accessor = accessor;
            _logger = logger;
        }

        public IEnumerable<TRange> JoinDataRangesToDataRanges(IEnumerable<TRange> dataRanges, IEnumerable<TRange> newDataRanges)
        {
            if (newDataRanges == null || !newDataRanges.Any())
            {
                _logger.LogInformation("No new data ranges");
                return dataRanges;
            }

            if (dataRanges == null)
            {
                _logger.LogInformation("Null existing data ranges");
                return newDataRanges;
            }

            IEnumerable<TRange> joined = dataRanges;

            _logger.LogInformation($"{dataRanges.Count()} existing data ranges");
            _logger.LogInformation($"{newDataRanges.Count()} new data ranges");

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
                _logger.LogInformation($"No ranges to join to {ranges}");
                return new [] { newRange };
            }

            if (newRange == null)
            {
                _logger.LogInformation($"No new range");
                return ranges;
            }
            
            return ranges.JoinInsert(newRange, PlaceDataRange, JoinTwoDataRanges);
        }

        private InsertPlacement PlaceDataRange(TRange itemToInsert, TRange existingItem)
        {
            var insertTimeRange = _accessor.GetTimeRange(itemToInsert);
            var existingTimeRange = _accessor.GetTimeRange(existingItem);

            if (insertTimeRange.Touches(existingTimeRange))
            {
                return InsertPlacement.Equal;
            }

            if (insertTimeRange.Min < existingTimeRange.Min)
            {
                return InsertPlacement.Before;
            }

            if (insertTimeRange.Min > existingTimeRange.Min)
            {
                return InsertPlacement.After;
            }

            throw new Exception("PlaceDataRange can't place data range");
        }

        private TRange JoinTwoDataRanges(TRange old, TRange insert)
        {
            var insertTimeRange = _accessor.GetTimeRange(insert);
            var oldTimeRange = _accessor.GetTimeRange(old);

            var min = Math.Min(insertTimeRange.Min, oldTimeRange.Min);
            var max = Math.Max(insertTimeRange.Max, oldTimeRange.Max);

            var data = JoinData(_accessor.GetData(insert), _accessor.GetData(old));
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