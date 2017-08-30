﻿using System;
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
        public async Task<IEnumerable<DataRange>> GetAttributeData(Guid entity, Guid attribute, int aggregationSeconds, [FromQuery] long minTimeSecondsUtc, [FromQuery] long maxTimeSecondsUtc)
        {
            var timeRange = new TimeRange(minTimeSecondsUtc, maxTimeSecondsUtc);
            IAttributeSeriesGrain series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(entity, attribute));
            return await series.GetData(timeRange, aggregationSeconds);
        }

        [HttpPost("/api/Entities/{entity}/Attributes/{attribute}/Aggregations/{aggregationSeconds}/Data")]
        public Task AddData(Guid entity, Guid attribute, int aggregationSeconds, [FromBody] IEnumerable<DataRange> dataRanges)
        {
            IAttributeSeriesGrain series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(entity, attribute));
            return series.AddData(dataRanges, aggregationSeconds);
        }
    }
}