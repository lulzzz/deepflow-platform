using Deepflow.Platform.Abstractions.Series;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Model;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Deepflow.Platform.Series.Sources
{
    public class SourceSeriesGrain : Grain, ISourceSeriesGrain
    {
        private readonly IModelMapProvider _mapProvider;
        private readonly ILogger<SourceSeriesGrain> _logger;
        private Guid _dataSource;
        private string _sourceName;

        public SourceSeriesGrain(IModelMapProvider mapProvider, ILogger<SourceSeriesGrain> logger)
        {
            _mapProvider = mapProvider;
            _logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            var key = this.GetPrimaryKeyString();
            var parts = key.Split(':');
            _dataSource = Guid.Parse(parts[0]);
            _sourceName = parts[1];
            await base.OnActivateAsync();
        }

        public Task AddAggregatedData(AggregatedDataRange dataRange, int aggregationSeconds)
        {
            return AddAggregatedData(new List<AggregatedDataRange> { dataRange }, aggregationSeconds);
        }

        public async Task AddAggregatedData(IEnumerable<AggregatedDataRange> dataRanges, int aggregationSeconds)
        {
            try
            {
                _logger.LogInformation($"Preparing to add data");
                var seriesMapping = await _mapProvider.GetSeriesModelMapping(_dataSource, _sourceName);
                IAttributeSeriesGrain series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(seriesMapping.Entity, seriesMapping.Attribute));
                await series.AddAggregatedData(dataRanges, aggregationSeconds);
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception, "Error when adding aggregated data");
                throw;
            }
        }

        public async Task NotifyRawData(IEnumerable<RawDataRange> dataRanges)
        {
            var seriesMapping = await _mapProvider.GetSeriesModelMapping(_dataSource, _sourceName);
            IAttributeSeriesGrain series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(seriesMapping.Entity, seriesMapping.Attribute));
            await series.NotifyRawData(dataRanges);
        }
    }
}
