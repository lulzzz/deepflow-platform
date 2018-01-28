using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Common.Model.Model;
using Deepflow.Ingestion.Service.Configuration;
using Deepflow.Platform.Abstractions.Ingestion;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Sources;
using Deepflow.Platform.Common.Data.Persistence;
using Deepflow.Platform.Core.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IIngestionProcessor = Deepflow.Ingestion.Service.Processing.IIngestionProcessor;

namespace Deepflow.Ingestion.Service.Controllers
{
    [Route("api/v1/[controller]")]
    public class DataSourcesController : Controller
    {
        private readonly IModelProvider _modelProvider;
        private readonly IPersistentDataProvider _persistence;
        private readonly IRangeFilterer<TimeRange> _filterer;
        private readonly IngestionConfiguration _configuration;
        private readonly IIngestionProcessor _processor;
        private readonly IModelProvider _model;

        public DataSourcesController(ILogger<DataSourcesController> logger, IModelProvider modelProvider, IPersistentDataProvider persistence, IRangeFilterer<TimeRange> filterer, IngestionConfiguration configuration, IIngestionProcessor processor, IModelProvider model)
        {
            _modelProvider = modelProvider;
            _persistence = persistence;
            _filterer = filterer;
            _configuration = configuration;
            _processor = processor;
            _model = model;
        }

        [HttpGet("{dataSource}/Tags")]
        public async Task<SourceSeriesList> GetSourceSeriesList(Guid dataSource)
        {
            var sourceNames = await _modelProvider.ResolveSourceNamesForDataSource(dataSource);

            return new SourceSeriesList
            {
                AggregationSeconds = 300,
                Series = await Task.WhenAll(sourceNames.Select(async x => new SourceSeriesFetchRequests
                {
                    Realtime = true,
                    SourceName = x,
                    TimeRanges = await GetMissingTimeRangesForSourceName(dataSource, x)
                }))
            };
        }
        
        [HttpPost("{dataSource}/Tags/{sourceName}/Data/Aggregations/{aggregationSeconds}/Historical")]
        public async Task AddHistoricalAggregatedData(Guid dataSource, string sourceName, int aggregationSeconds, [FromBody] AggregatedDataRange dataRange)
        {
            var (entity, attribute) = await _model.ResolveEntityAndAttribute(dataSource, sourceName);
            await _processor.ReceiveHistoricalData(entity, attribute, dataRange);
        }

        [HttpPost("{dataSource}/Tags/{sourceName}/Data/Raw/Realtime")]
        public async Task AddRealtimeRawData(Guid dataSource, string sourceName, int aggregationSeconds, [FromBody] RawDataRange dataRange)
        {
            var (entity, attribute) = await _model.ResolveEntityAndAttribute(dataSource, sourceName);
            await _processor.ReceiveRealtimeRawData(entity, attribute, dataRange);
        }

        [HttpPost("{dataSource}/Tags/{sourceName}/Data/Aggregations/{aggregationSeconds}/Realtime")]
        public async Task AddRealtimeAggregatedData(Guid dataSource, string sourceName, int aggregationSeconds, [FromBody] AggregatedDataRange dataRange)
        {
            var (entity, attribute) = await _model.ResolveEntityAndAttribute(dataSource, sourceName);
            await _processor.ReceiveRealtimeAggregatedData(entity, attribute, dataRange);
        }

        private async Task<IEnumerable<TimeRange>> GetMissingTimeRangesForSourceName(Guid dataSource, string sourceName)
        {
            var entityAttribute = await _modelProvider.ResolveEntityAndAttribute(dataSource, sourceName);

            var existingTimeRanges = await _persistence.GetAllTimeRanges(entityAttribute.Item1, entityAttribute.Item2);
            var desiredTimeRange = new TimeRange(_configuration.MinHistoryUtcSeconds.ToDateTime(), DateTime.UtcNow);
            return _filterer.SubtractTimeRangesFromRange(desiredTimeRange, existingTimeRanges);
        }

        
    }
}
