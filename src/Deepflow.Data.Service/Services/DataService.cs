using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Common.Model.Model;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Common.Data.Caching;
using Deepflow.Platform.Common.Data.Persistence;

namespace Deepflow.Data.Service.Services
{
    public class DataService : IDataService
    {
        private readonly ICachedDataProvider _cache;
        private readonly IPersistentDataProvider _persistence;
        private readonly IModelProvider _model;
        private readonly IRangeFilterer<TimeRange> _filterer;
        private readonly IRangeMerger<AggregatedDataRange> _merger;

        public DataService(ICachedDataProvider cache, IPersistentDataProvider persistence, IModelProvider model, IRangeFilterer<TimeRange> filterer, IRangeMerger<AggregatedDataRange> merger)
        {
            _cache = cache;
            _persistence = persistence;
            _model = model;
            _filterer = filterer;
            _merger = merger;
        }

        public async Task<IEnumerable<AggregatedDataRange>> GetData(Guid entity, Guid attribute, TimeRange timeRange, int aggregationSeconds)
        {
            /*var cached = await _cache.GetAggregatedDataWithEdges(series, timeRange, aggregationSeconds);
            var notCovered = _filterer.FilterRanges(cached.Select(x => x.TimeRange), timeRange);
            if (!notCovered.Any())
            {
                return cached;
            }*/
            return await _persistence.GetAggregatedDataWithEdges(entity, attribute, aggregationSeconds, timeRange);
            //return _merger.MergeRangesWithRanges(cached, persisted.SelectMany(x => x));
        }
    }
}
