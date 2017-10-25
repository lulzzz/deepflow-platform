using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Sources.FakeSource.Data;
using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Platform.Sources.FakeSource.Controllers
{
    [Route("api/v1/[controller]")]
    public class DataController : Controller
    {
        private readonly IDataGenerator _generator;
        private readonly IDataAggregator _aggregator;
        private readonly GeneratorConfiguration _configuration;

        public DataController(IDataGenerator generator, IDataAggregator aggregator, GeneratorConfiguration configuration)
        {
            _generator = generator;
            _aggregator = aggregator;
            _configuration = configuration;
        }

        [HttpGet("{sourceName}/Aggregations/{aggregationSeconds}")]
        public AggregatedDataRange GetDataRange(string sourceName, int aggregationSeconds, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            var timeRange = new TimeRange(minTimeUtc.SecondsSince1970Utc(), maxTimeUtc.SecondsSince1970Utc());
            return _generator.GenerateRange(sourceName, timeRange, aggregationSeconds);
        }

        [HttpGet("{sourceName}/Raw")]
        public RawDataRange GetDataRange(string sourceName, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            var timeRange = new TimeRange(minTimeUtc.SecondsSince1970Utc(), maxTimeUtc.SecondsSince1970Utc());
            return _generator.GenerateRawRange(sourceName, timeRange, 5);
        }
    }
}
