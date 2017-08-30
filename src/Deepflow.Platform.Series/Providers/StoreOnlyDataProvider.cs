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

        protected override Task<IEnumerable<DataRange>> ProduceAttributeRanges(Guid series, IEnumerable<TimeRange> timeRanges)
        {
            return Task.FromResult((IEnumerable<DataRange>) new List<DataRange>());
        }
    }
}
