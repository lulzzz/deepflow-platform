using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Ingestion;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using Microsoft.Extensions.Logging;
using Orleans;

namespace Deepflow.Platform.Ingestion
{
    public class IngestionProcessor : IIngestionProcessor
    {
        private readonly IngestionConfiguration _configuration;
        private readonly ILogger<IngestionProcessor> _logger;

        public IngestionProcessor(IngestionConfiguration configuration, ILogger<IngestionProcessor> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task AddData(Guid dataSource, string sourceName, AggregatedDataRange aggregatedRange)
        {
            try
            {
                ISourceSeriesGrain series = GrainClient.GrainFactory.GetGrain<ISourceSeriesGrain>(SeriesIdHelper.ToSourceSeriesId(dataSource, sourceName));

                /*foreach (var dataRange in aggregatedRanges)
                {
                    var slices = dataRange.Chop(_configuration.MaxRangeSeconds);
                    foreach (var slice in slices)
                    {*/
                        _logger.LogInformation("Saving slice");
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        await series.AddData(aggregatedRange);
                        _logger.LogInformation($"Saved slice in {stopwatch.ElapsedMilliseconds} ms");
                    /*}
                }*/
            }
            catch (Exception exception)
            {
                _logger.LogError(null, exception, "Error when adding aggregated ranges");
                throw;
            }
        }
    }
}
