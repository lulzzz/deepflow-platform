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

        public DataController(IDataGenerator generator, IDataAggregator aggregator)
        {
            _generator = generator;
            _aggregator = aggregator;
        }

        [HttpGet("{sourceName}/Aggregations/{aggregationSeconds}")]
        public AggregatedDataRange GetDataRange(string sourceName, int aggregationSeconds, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            var timeRange = new TimeRange(minTimeUtc.SecondsSince1970Utc(), maxTimeUtc.SecondsSince1970Utc());
            var quantised = timeRange.Quantise(aggregationSeconds);
            var raw = _generator.GenerateData(sourceName, quantised, aggregationSeconds);
            var aggregatedData = new List<double>(raw.Data.Count);
            for (var i = 0; i < raw.Data.Count / 2; i++)
            {
                var time = raw.Data[i * 2];
                var value = raw.Data[i * 2 + 1];
                if (time != raw.TimeRange.Min)
                {
                    aggregatedData.Add(time);
                    aggregatedData.Add(value);
                }
            }
            return new AggregatedDataRange(raw.TimeRange, aggregatedData, aggregationSeconds);
        }
    }
}
