using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Common.Model.Model;
using Deepflow.Ingestion.Service.Configuration;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Sources;
using Deepflow.Platform.Common.Data.Persistence;
using Deepflow.Platform.Core.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Deepflow.Ingestion.Service.Controllers
{
    [Route("api/v1/[controller]")]
    public class DataSourcesController : Controller
    {
        private readonly IModelProvider _modelProvider;
        private readonly IPersistentDataProvider _persistence;
        private readonly IRangeFilterer<TimeRange> _filterer;
        private readonly IngestionConfiguration _configuration;

        public DataSourcesController(ILogger<DataSourcesController> logger, IModelProvider modelProvider, IPersistentDataProvider persistence, IRangeFilterer<TimeRange> filterer, IngestionConfiguration configuration)
        {
            _modelProvider = modelProvider;
            _persistence = persistence;
            _filterer = filterer;
            _configuration = configuration;
        }

        [HttpGet("{dataSource}/Series")]
        public async Task<SourceSeriesList> GetSourceSeriesList(Guid dataSource)
        {
            var sourceNames = await _modelProvider.ResolveSourceNamesForDataSource(dataSource);
            var aggregationSeconds = 300;

            return new SourceSeriesList
            {
                AggregationSeconds = 300,
                Series = await Task.WhenAll(sourceNames.Select(async x => new SourceSeriesFetchRequests
                {
                    Realtime = true,
                    SourceName = x,
                    TimeRanges = await GetMissingTimeRangesForSourceName(dataSource, x, aggregationSeconds)
                }))
            };
        }

        private async Task<IEnumerable<TimeRange>> GetMissingTimeRangesForSourceName(Guid dataSource, string sourceName, int aggregationSeconds)
        {
            var entityAttribute = await _modelProvider.ResolveEntityAndAttribute(dataSource, sourceName);
            var series = await _modelProvider.ResolveSeries(entityAttribute.entity, entityAttribute.attribute, aggregationSeconds);

            var existingTimeRanges = await _persistence.GetAllTimeRanges(series);
            var desiredTimeRange = new TimeRange(_configuration.MinHistoryUtcSeconds.FromSecondsSince1970Utc(), DateTime.UtcNow);
            return _filterer.SubtractTimeRangesFromRange(desiredTimeRange, existingTimeRanges);
        }
    }
}
