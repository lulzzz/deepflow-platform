using System;
using System.Collections.Generic;
using System.Text;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Attribute;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Series.Attributes
{
    public class AttributeDataProviderFactory : IAttributeDataProviderFactory
    {
        private readonly IDataStore _store;
        private readonly IDataValidator _validator;
        private readonly IDataAggregator _aggregator;
        private readonly ISeriesKnower _seriesKnower;
        private readonly IRangeFilterer<AggregatedDataRange> _filterer;
        private readonly IRangeMerger<AggregatedDataRange> _aggregatedMerger;
        private readonly IRangeMerger<TimeRange> _timeMerger;
        private readonly ISeriesConfiguration _configuration;
        private readonly ILogger<AttributeDataProvider> _logger;

        public AttributeDataProviderFactory(IDataStore store, IDataValidator validator, IDataAggregator aggregator, ISeriesKnower seriesKnower, IRangeFilterer<AggregatedDataRange> filterer, IRangeMerger<AggregatedDataRange> aggregatedMerger, IRangeMerger<TimeRange> timeMerger, ISeriesConfiguration configuration, ILogger<AttributeDataProvider> logger)
        {
            _store = store;
            _validator = validator;
            _aggregator = aggregator;
            _seriesKnower = seriesKnower;
            _filterer = filterer;
            _aggregatedMerger = aggregatedMerger;
            _timeMerger = timeMerger;
            _configuration = configuration;
            _logger = logger;
        }

        public IAttributeDataProvider Create(Guid entity, Guid attribute)
        {
            return new AttributeDataProvider(entity, attribute, _store, _validator, _aggregator, _seriesKnower, _filterer, _aggregatedMerger, _timeMerger, _configuration, _logger);
        }
    }
}
