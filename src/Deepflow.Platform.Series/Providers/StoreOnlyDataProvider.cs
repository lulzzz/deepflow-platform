/*
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;

namespace Deepflow.Platform.Series.Providers
{
    public class StoreOnlyDataProvider : InMemoryDataProvider
    {
        public StoreOnlyDataProvider(IDataMerger merger, IDataFilterer dataFilterer, ITimeFilterer timeFilterer) : base(merger, dataFilterer, timeFilterer)
        {
        }

        protected override Task<IEnumerable<RawDataRange>> ProduceAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            return Task.FromResult((IEnumerable<RawDataRange>) new List<RawDataRange>());
        }
    }
}
*/
