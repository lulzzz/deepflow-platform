using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Orleans.Runtime.Configuration;

namespace Deepflow.Platform.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        [HttpGet("Aggregated")]
        public async Task<IEnumerable<DataRange>> GetAggregatedData([FromQuery] Guid entity, [FromQuery] Guid attribute, [FromQuery] long minTimeSeconds, [FromQuery] long maxTimeSeconds, [FromQuery] int aggregationSeconds)
        {
            var stopwatch = Stopwatch.StartNew();
            var timeRange = new TimeRange(minTimeSeconds, maxTimeSeconds);
            ISeriesGrain series = GrainClient.GrainFactory.GetGrain<ISeriesGrain>(SeriesIdHelper.ToSeriesId(Guid.NewGuid(), Guid.NewGuid()));
            var result = await series.GetAggregatedData(timeRange, aggregationSeconds);
            return result;
            return new List<DataRange> { new DataRange(minTimeSeconds, maxTimeSeconds, new List<double>{ stopwatch.ElapsedMilliseconds }) };
        }
        
        [HttpPost("Raw")]
        public Task AddRawData([FromQuery] Guid entity, [FromQuery] Guid attribute, [FromBody] IEnumerable<DataRange> dataRanges)
        {
            ISeriesGrain series = GrainClient.GrainFactory.GetGrain<ISeriesGrain>(SeriesIdHelper.ToSeriesId(entity, attribute));
            return series.AddRawData(dataRanges);
        }
    }
}