using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Validators;
using Microsoft.AspNetCore.Mvc;
using Deepflow.Platform.Agent.Processor;
using FluentValidation;

namespace Deepflow.Platform.Agent.Controllers
{
    [Route("api/v1/[controller]")]
    public class TagsController : Controller
    {
        private readonly IAgentProcessor _processor;
        private readonly RawDataRangeValidator _validator = new RawDataRangeValidator();

        public TagsController(IAgentProcessor processor)
        {
            _processor = processor;
        }

        [HttpPost("{sourceName}/Raw/Data")]
        public Task NotifyRealtime(string sourceName, [FromBody] RawDataRange dataRange)
        {
            _validator.ValidateAndThrow(dataRange);
            return _processor.ReceiveRealtimeRaw(sourceName, dataRange);
        }
    }
}