using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Ingestion;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Core.Tools;
using Deepflow.Platform.Series;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Ingestion
{
    public class IngestionProcessor : IIngestionProcessor
    {
        private readonly IngestionConfiguration _configuration;
        private readonly ILogger<IngestionProcessor> _logger;
        private readonly TripCounterFactory _tripCounterFactory;

        public IngestionProcessor(IngestionConfiguration configuration, ILogger<IngestionProcessor> logger, TripCounterFactory tripCounterFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _tripCounterFactory = tripCounterFactory;
        }

        public async Task AddData(Guid dataSource, string sourceName, AggregatedDataRange aggregatedRange)
        {
            try
            {
                await _tripCounterFactory.Run("IngestionProcessor.AddData", () =>
                {
                    return Task.CompletedTask;
                    /*ISourceSeriesGrain series = GrainClient.GrainFactory.GetGrain<ISourceSeriesGrain>(SeriesIdHelper.ToSourceSeriesId(dataSource, sourceName));
                    _logger.LogDebug("Saving slice");
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    await series.AddData(aggregatedRange);
                    _logger.LogDebug($"Saved slice number {Interlocked.Increment(ref _count)} in {stopwatch.ElapsedMilliseconds} ms");*/
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(new EventId(108), exception, "Error when adding aggregated ranges");
                throw;
            }
        }
    }
}
