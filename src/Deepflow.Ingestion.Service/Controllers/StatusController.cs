using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Ingestion.Service.Controllers
{
    [Route("api/v1/[controller]")]
    public class StatusController : Controller
    {
        [HttpGet]
        public string GetStatus()
        {
            return "OK";
        }
    }
}
