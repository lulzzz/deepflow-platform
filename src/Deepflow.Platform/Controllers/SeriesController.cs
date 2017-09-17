using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Series;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Deepflow.Platform.Controllers
{
    public class SeriesController : Controller
    {
        [HttpGet("/api/Entities/{entity}/Attributes/{attribute}/Aggregations/{aggregationSeconds}/Data")]
        public async Task<IEnumerable<AggregatedDataRange>> GetAggregatedAttributeData(Guid entity, Guid attribute, int aggregationSeconds, [FromQuery] long minTimeSecondsUtc, [FromQuery] long maxTimeSecondsUtc)
        {
            var timeRange = new TimeRange(minTimeSecondsUtc, maxTimeSecondsUtc);
            IAttributeSeriesGrain series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(entity, attribute));
            var data = await series.GetAggregatedData(timeRange, aggregationSeconds);
            return data;
        }
    }
}