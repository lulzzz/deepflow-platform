using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Data.Service.Services;
using Deepflow.Platform.Abstractions.Series;
using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Data.Service.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly IDataService _data;

        public DataController(IDataService data)
        {
            _data = data;
        }

        [HttpGet("{entity}/{attribute}/{aggregationSeconds}")]
        public Task<IEnumerable<AggregatedDataRange>> GetData(Guid entity, Guid attribute, [FromQuery] DateTime minUtc, [FromQuery] DateTime maxUtc, int aggregationSeconds)
        {
            return _data.GetData(entity, attribute, new TimeRange(minUtc, maxUtc), aggregationSeconds);
        }
    }
}
