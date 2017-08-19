using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Series;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Deepflow.Platform.Controllers
{
    /*[Route("api/[controller]")]*/
    public class SeriesController : Controller
    {
        [HttpGet("/api/Entities/{entity}/Attributes/{attribute}/Aggregations/{aggregationSeconds}/Data")]
        public async Task<DataRange> GetAttributeData(Guid entity, Guid attribute, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc, int aggregationSeconds)
        {
            var timeRange = new TimeRange(minTimeUtc.SecondsSince1970Utc(), maxTimeUtc.SecondsSince1970Utc());
            IAttributeSeriesGrain series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(entity, attribute));
            return await series.GetData(timeRange, aggregationSeconds);
        }
        
        /*[HttpPost("Raw")]
        public Task AddRawData([FromQuery] Guid entity, [FromQuery] Guid attribute, [FromBody] IEnumerable<DataRange> dataRanges)
        {
            ISeriesGrain series = GrainClient.GrainFactory.GetGrain<ISeriesGrain>(SeriesIdHelper.ToSeriesId(entity, attribute));
            return series.AddRawData(dataRanges);
        }*/
    }
}