using System;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Sources.FakeSource.Data;
using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Platform.Sources.FakeSource.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly DataGenerator _generator = new DataGenerator();

        [HttpGet("{sourceName}/Aggregations/{aggregationSeconds}")]
        public DataRange GetDataRange(string sourceName, int aggregationSeconds, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            var timeRange = new TimeRange(minTimeUtc.SecondsSince1970Utc(), maxTimeUtc.SecondsSince1970Utc());
            return _generator.GenerateData(sourceName, timeRange, aggregationSeconds);
        }
    }
}
