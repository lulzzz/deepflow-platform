using System;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Agent.Provider;
using Microsoft.AspNetCore.Mvc;
using Deepflow.Platform.Agent.Processor;

namespace Deepflow.Platform.Agent.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly ISourceDataProvider _data;
        private readonly IAgentProcessor _processor;

        public DataController(ISourceDataProvider data, IAgentProcessor processor)
        {
            _data = data;
            _processor = processor;
        }

        [HttpGet("{sourceName}/Raw")]
        public Task<RawDataRange> GetData(string sourceName, [FromQuery] DateTime minUtc, [FromQuery] DateTime maxUtc)
        {
            return _data.FetchRawData(sourceName, new TimeRange(minUtc, maxUtc));
        }

        [HttpPost("{sourceName}/Raw")]
        public Task NotifyRealtime(string sourceName, RawDataRange dataRange)
        {
            return _processor.ReceiveRaw(sourceName, dataRange);
        }
    }
}
