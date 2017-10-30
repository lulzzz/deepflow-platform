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
        private readonly IRangeFilterer<AggregatedDataRange> _aggregatedFilterer;
        private readonly IRangeFilterer<RawDataRange> _rawFilterer;

        public DataController(IDataGenerator generator, IDataAggregator aggregator, GeneratorConfiguration configuration, IRangeFilterer<AggregatedDataRange> aggregatedFilterer, IRangeFilterer<RawDataRange> rawFilterer)
        {
            _generator = generator;
            _aggregatedFilterer = aggregatedFilterer;
            _rawFilterer = rawFilterer;
        }

        [HttpGet("{sourceName}/Aggregations/{aggregationSeconds}")]
        public AggregatedDataRange GetDataRange(string sourceName, int aggregationSeconds, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            var timeRange = new TimeRange(minTimeUtc.SecondsSince1970Utc(), maxTimeUtc.SecondsSince1970Utc());
            var data = _generator.GenerateRange(sourceName, timeRange, aggregationSeconds);
            var now = DateTime.UtcNow.SecondsSince1970Utc();
            if (data.TimeRange.Min >= now)
            {
                return new AggregatedDataRange(timeRange, new List<double>(), aggregationSeconds);
            }
            var filteredData = _aggregatedFilterer.FilterDataRange(data, new TimeRange(data.TimeRange.Min, now));
            return new AggregatedDataRange(timeRange, filteredData.Data, aggregationSeconds);
        }

        [HttpGet("{sourceName}/Raw")]
        public RawDataRange GetDataRange(string sourceName, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            var timeRange = new TimeRange(minTimeUtc.SecondsSince1970Utc(), maxTimeUtc.SecondsSince1970Utc());
            var data = _generator.GenerateRawRange(sourceName, timeRange, 5);
            var now = DateTime.UtcNow.SecondsSince1970Utc();
            if (data.TimeRange.Min >= now)
            {
                return new RawDataRange(timeRange, new List<double>());
            }
            var filteredData = _rawFilterer.FilterDataRange(data, new TimeRange(data.TimeRange.Min, now));
            return new RawDataRange(timeRange, filteredData.Data);
        }
    }
}
