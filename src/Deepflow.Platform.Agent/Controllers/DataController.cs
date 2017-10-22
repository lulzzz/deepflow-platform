using System;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Agent.Provider;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Platform.Agent.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly ISourceDataProvider _data;

        public DataController(ISourceDataProvider data)
        {
            _data = data;
        }

        [HttpGet("{sourceName}/Raw")]
        public Task<RawDataRange> GetData(string sourceName, [FromQuery] DateTime minUtc, [FromQuery] DateTime maxUtc)
        {
            return _data.FetchRawData(sourceName, new TimeRange(minUtc, maxUtc));
        }
    }
}
