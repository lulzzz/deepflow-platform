using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Sources;
using Deepflow.Platform.Core.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Ingestion.FakeIngestor.Controllers
{
    [Route("api/v1/[controller]")]
    public class DataSourcesController : Controller, IDataSourcesController
    {
        private readonly ILogger<DataSourcesController> _logger;

        public DataSourcesController(ILogger<DataSourcesController> logger)
        {
            _logger = logger;
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
        public Task AddAggregatedRanges(Guid dataSource, string sourceName, int aggregationSeconds, [FromBody] IEnumerable<AggregatedDataRange> dataRanges)
        {
            _logger.LogInformation($"Received data for {dataSource}/Series/{sourceName}/Aggregations/{aggregationSeconds}/Data with {dataRanges.Count()} ranges with {dataRanges.Sum(x => x.Data.Count / 2)} points");
            return Task.FromResult(0);
        }

        [HttpPost("{dataSource}/Series/{sourceName}/Raw/Data")]
        public Task NotifyRawRanges(Guid dataSource, string sourceName, [FromBody] IEnumerable<RawDataRange> dataRanges)
        {
            _logger.LogInformation($"Received data for {dataSource}/Series/{sourceName}/Raw/Data with {dataRanges.Count()} ranges with {dataRanges.Sum(x => x.Data.Count / 2)} points");
            return Task.FromResult(0);
        }
    }
}
