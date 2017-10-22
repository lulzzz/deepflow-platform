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
using Orleans;

namespace Deepflow.Platform.Ingestion
{
    public class IngestionProcessor : IIngestionProcessor
    {
        private readonly IngestionConfiguration _configuration;
        private readonly ILogger<IngestionProcessor> _logger;
        private readonly TripCounterFactory _tripCounterFactory;
        private static int _count = 0;

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
                using (_tripCounterFactory.Create("IngestionProcessor.AddData"))
                {
                    ISourceSeriesGrain series = GrainClient.GrainFactory.GetGrain<ISourceSeriesGrain>(SeriesIdHelper.ToSourceSeriesId(dataSource, sourceName));

                    /*foreach (var dataRange in aggregatedRanges)
                    {
                        var slices = dataRange.Chop(_configuration.MaxRangeSeconds);
                        foreach (var slice in slices)
                        {*/
                    _logger.LogDebug("Saving slice");
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    await series.AddData(aggregatedRange);
                    _logger.LogDebug($"Saved slice number {Interlocked.Increment(ref _count)} in {stopwatch.ElapsedMilliseconds} ms");
                    /*}
                }*/
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(new EventId(108), exception, "Error when adding aggregated ranges");
                throw;
            }
        }
    }
}
