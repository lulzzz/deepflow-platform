/*
using System;
using System.Threading.Tasks;
using Deepflow.Common.Model.Model;
using Deepflow.Platform.Abstractions.Ingestion;
using Microsoft.AspNetCore.Mvc;
using IIngestionProcessor = Deepflow.Ingestion.Service.Processing.IIngestionProcessor;

namespace Deepflow.Ingestion.Service.Controllers
{
    [Route("api/v1/[controller]")]
    public class IngestionController : Controller
    {
        private readonly IIngestionProcessor _processor;
        private readonly IModelProvider _model;

        public IngestionController(IIngestionProcessor processor, IModelProvider model)
        {
            _processor = processor;
            _model = model;
        }

        [HttpPost("DataSources/{dataSource}/Series/{sourceName}")]
        public async Task ReceiveRealtimeData(Guid dataSource, string sourceName, [FromBody] IngestionPackage package)
        {
            var (entity, attribute) = await _model.ResolveEntityAndAttribute(dataSource, sourceName);
            await _processor.ReceiveRealtimeData(entity, attribute, package.AggregatedDataRange, package.RawDataRange);
        }
    }
}
*/
