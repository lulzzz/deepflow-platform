using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Common.Model.Model;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Common.Data.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Ingestion.Service.Controllers
{
    [Route("api/v1/[controller]")]
    public class EntitiesController : Controller
    {
        private readonly IPersistentDataProvider _persistence;
        private readonly IModelProvider _model;

        public EntitiesController(IPersistentDataProvider persistence, IModelProvider model)
        {
            _persistence = persistence;
            _model = model;
        }

        [HttpGet("{entity}/Attributes/{attribute}/Aggregations/{aggregationSeconds}/Data")]
        public async Task<IEnumerable<AggregatedDataRange>> GetAggregatedData(Guid entity, Guid attribute, int aggregationSeconds, [FromQuery] DateTimeOffset minTime, [FromQuery] DateTimeOffset maxTime)
        {
            return await _persistence.GetData(entity, attribute, aggregationSeconds, new TimeRange(minTime.UtcDateTime, maxTime.UtcDateTime));
        }
    }
}
