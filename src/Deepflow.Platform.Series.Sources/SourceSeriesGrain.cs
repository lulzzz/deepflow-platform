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

        public Task AddAggregatedData(DataRange dataRange, int aggregationSeconds)
        {
            return AddAggregatedData(new List<DataRange> { dataRange }, aggregationSeconds);
        }

        public async Task AddAggregatedData(IEnumerable<DataRange> dataRanges, int aggregationSeconds)
        {
            _logger.LogInformation($"Preparing to add data");
            var seriesMapping = await _mapProvider.GetSeriesModelMapping(_dataSource, _sourceName);
            IAttributeSeriesGrain series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(seriesMapping.Entity, seriesMapping.Attribute));
            await series.AddAggregatedData(dataRanges, aggregationSeconds);
        }

        public async Task NotifyRawData(IEnumerable<DataRange> dataRanges)
        {
            var seriesMapping = await _mapProvider.GetSeriesModelMapping(_dataSource, _sourceName);
            IAttributeSeriesGrain series = GrainClient.GrainFactory.GetGrain<IAttributeSeriesGrain>(SeriesIdHelper.ToAttributeSeriesId(seriesMapping.Entity, seriesMapping.Attribute));
            await series.NotifyRawData(dataRanges);
        }
    }
}
