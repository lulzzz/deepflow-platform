using Deepflow.Common.Model;
using Deepflow.Common.Model.Model;
using Microsoft.AspNetCore.Mvc;

namespace Deepflow.Ingestion.Service.Controllers
{
    [Route("api/v1/[controller]")]
    public class ModelController : Controller
    {
        private readonly IModelProvider _provider;

        public ModelController(IModelProvider provider)
        {
            _provider = provider;
        }

        [HttpGet]
        public ModelConfiguration GetModel()
        {
            return _provider.GetModelConfiguration();
        }
    }
}
