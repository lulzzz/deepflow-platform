using System;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Sources.PISim.Provider;
using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Platform.Sources.PISim.Controllers
{
    [Route("api/v1/[controller]")]
    public class DataController : Controller
    {
        private readonly IPiSimDataProvider _piSimDataProvider;

        public DataController(IPiSimDataProvider piSimDataProvider)
        {
            _piSimDataProvider = piSimDataProvider;
        }

        /*[HttpGet("{sourceName}/Aggregations/{aggregationSeconds}")]
        public Task<AggregatedDataRange> GetAggregatedDataRange(string sourceName, int aggregationSeconds, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            return _piSimDataProvider.GetAggregatedDataRange(sourceName, aggregationSeconds, minTimeUtc, maxTimeUtc);
        }

        [HttpGet("{sourceName}/Raw")]
        public Task<RawDataRange> GetRawDataRangeWithEdges(string sourceName, [FromQuery] DateTime minTimeUtc, [FromQuery] DateTime maxTimeUtc)
        {
            return _piSimDataProvider.GetRawDataRangeWithEdges(sourceName, minTimeUtc, maxTimeUtc);
        }*/
    }
}
