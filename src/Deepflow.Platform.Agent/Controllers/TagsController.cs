using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Microsoft.AspNetCore.Mvc;
using Deepflow.Platform.Agent.Processor;

namespace Deepflow.Platform.Agent.Controllers
{
    [Route("api/v1/[controller]")]
    public class TagsController : Controller
    {
        private readonly IAgentProcessor _processor;

        public TagsController(IAgentProcessor processor)
        {
            _processor = processor;
        }

        [HttpPost("{sourceName}/Raw/Data")]
        public Task NotifyRealtime(string sourceName, [FromBody] RawDataRange dataRange)
        {
            return _processor.ReceiveRealtimeRaw(sourceName, dataRange);
        }
    }
}