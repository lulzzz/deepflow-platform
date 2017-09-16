using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Agent.Model;
using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Platform.Agent.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        [HttpPost]
        public void NotifyRaw(List<RawDataRange> dataRanges)
        {
            
        }
    }
}
