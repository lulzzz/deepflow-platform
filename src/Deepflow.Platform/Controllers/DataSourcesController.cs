using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Ingestion;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Sources;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Series;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Deepflow.Platform.Controllers
{
    [Route("api/v1/[controller]")]
    public class DataSourcesController : Controller, IDataSourcesController
    {
        private readonly ILogger<DataSourcesController> _logger;
        private readonly IIngestionProcessor _processor;

        public DataSourcesController(ILogger<DataSourcesController> logger, IIngestionProcessor processor)
        {
            _logger = logger;
            _processor = processor;
        }

        [HttpGet("{dataSource}/Series")]
        public Task<SourceSeriesList> GetSourceSeriesList(Guid dataSource)
        {
            return Task.FromResult(new SourceSeriesList
            {
                AggregationSeconds = 300,
                Series = new List<SourceSeriesFetchRequests>(
                    Enumerable.Range(0, 500).Select(x => new SourceSeriesFetchRequests
                    {
                        Realtime = true,
                        SourceName = "tag" + x,
                        TimeRanges = new List<TimeRange>
                        {
                            //new TimeRange(new DateTime(2005, 1, 1).SecondsSince1970Utc(), new DateTime(2015, 1, 1).SecondsSince1970Utc())
                        }
                    }))
            });
        }

        [HttpPost("{dataSource}/Series/{sourceName}/Aggregations/{aggregationSeconds}/Data")]
        public Task AddAggregatedRanges(Guid dataSource, string sourceName, int aggregationSeconds, [FromBody] IEnumerable<DataRange> dataRanges)
        {
            dataRanges.ForEach(dataRange => dataRange.Validate());
            return _processor.AddAggregatedRanges(dataSource, sourceName, aggregationSeconds, dataRanges);
        }

        [HttpPost("{dataSource}/Series/{sourceName}/Raw/Data")]
        public Task NotifyRawRanges(Guid dataSource, string sourceName, [FromBody] IEnumerable<DataRange> dataRanges)
        {
            dataRanges.ForEach(dataRange => dataRange.Validate());
            ISourceSeriesGrain series = GrainClient.GrainFactory.GetGrain<ISourceSeriesGrain>(SeriesIdHelper.ToSourceSeriesId(dataSource, sourceName));
            return series.NotifyRawData(dataRanges);
        }
    }
}
