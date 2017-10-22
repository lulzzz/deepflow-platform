using Deepflow.Platform.Abstractions.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Model;
using Deepflow.Platform.Abstractions.Series.Attribute;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;

namespace Deepflow.Platform.Series.Sources
{
    [Reentrant]
    public class SourceSeriesGrain : Grain, ISourceSeriesGrain
    {
        private readonly IModelMapProvider _mapProvider;
        private readonly ILogger<SourceSeriesGrain> _logger;
        private readonly TripCounterFactory _tripCounterFactory;
        private Guid _dataSource;
        private string _sourceName;

        public SourceSeriesGrain(IModelMapProvider mapProvider, ILogger<SourceSeriesGrain> logger, TripCounterFactory tripCounterFactory)
        {
            _mapProvider = mapProvider;
            _logger = logger;
            _tripCounterFactory = tripCounterFactory;
        }

        public override async Task OnActivateAsync()
        {
            var key = this.GetPrimaryKeyString();
            var parts = key.Split(':');
            _dataSource = Guid.Parse(parts[0]);
            _sourceName = string.Join(":", parts.Skip(1));
            await base.OnActivateAsync();
        }

        public async Task AddData(AggregatedDataRange aggregatedRange)
        {
            try
            {
                using (_tripCounterFactory.Create("SourceSeriesGrain.AddData"))
                {
                    _logger.LogDebug($"Preparing to add data");
                    var seriesMapping = await _mapProvider.GetSeriesModelMapping(_dataSource, _sourceName);
                    IAttributeSeriesGrain series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(seriesMapping.Entity, seriesMapping.Attribute));
                    await series.ReceiveData(aggregatedRange);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(new EventId(105), exception, "Error when adding aggregated data");
                throw;
            }
        }
    }
}
