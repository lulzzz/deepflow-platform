using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Ingestion;
using Deepflow.Platform.Abstractions.Model;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Sources;
using Deepflow.Platform.Series.Sources.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Controllers
{
    [Route("api/v1/[controller]")]
    public class DataSourcesController : Controller, IDataSourcesController
    {
        private readonly DataSourceDataPackageValidator _validator = new DataSourceDataPackageValidator();
        private readonly ILogger<DataSourcesController> _logger;
        private readonly IIngestionProcessor _processor;
        private readonly IModelMapProvider _modelMapProvider;

        public DataSourcesController(ILogger<DataSourcesController> logger, IIngestionProcessor processor, IModelMapProvider modelMapProvider)
        {
            _logger = logger;
            _processor = processor;
            _modelMapProvider = modelMapProvider;
        }

        [HttpGet("{dataSource}/Series")]
        public async Task<SourceSeriesList> GetSourceSeriesList(Guid dataSource)
        {
            var modelMap = await _modelMapProvider.GetSourceNamesForDataSource(dataSource);
            return new SourceSeriesList
            {
                AggregationSeconds = 300,
                Series = modelMap.Select(x => new SourceSeriesFetchRequests
                    {
                        Realtime = true,
                        SourceName = x,
                        TimeRanges = new List<TimeRange>
                        {
                            //new TimeRange(new DateTime(2005, 1, 1).SecondsSince1970Utc(), new DateTime(2015, 1, 1).SecondsSince1970Utc())
                        }
                    })
            };
        }

        [HttpPost("{dataSource}/Series/{sourceName}/Data")]
        public Task AddData(Guid dataSource, string sourceName, [FromBody] DataSourceDataPackage dataPackage)
        {
            try
            {
                _validator.ValidateAndThrow(dataPackage);

                _logger.LogDebug($"Received data from data source {dataSource} for {sourceName} with {dataPackage.AggregatedRange.Data.Count / 2} aggregated pointss");
                return _processor.AddData(dataSource, sourceName, dataPackage.AggregatedRange);
            }
            catch (Exception exception)
            {
                _logger.LogError(new EventId(101), exception, "Error when adding aggregated ranges");
                throw;
            }
        }
    }
}
