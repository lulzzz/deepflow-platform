using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Common.Model;
using Deepflow.Common.Model.Model;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Ingestion.Service.Realtime;
using Deepflow.Platform.Abstractions.Series.Validators;
using Deepflow.Platform.Common.Data.Persistence;
using Deepflow.Platform.Core.Tools;
using FluentValidation;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepflow.Ingestion.Service.Processing
{
    public class IngestionProcessor : IIngestionProcessor
    {
        private readonly IPersistentDataProvider _persistence;
        private readonly IDataAggregator _aggregator;
        private readonly IModelProvider _model;
        private readonly IDataMessenger _messenger;
        private readonly IRangeMerger<AggregatedDataRange> _aggregatedMerger;
        private readonly IRangeMerger<TimeRange> _timeMerger;
        private readonly IRangeFilterer<AggregatedDataRange> _filterer;
        private readonly SeriesConfiguration _configuration;
        private readonly ILogger<IngestionProcessor> _logger;
        private readonly TelemetryClient _telemetry = new TelemetryClient();

        private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _seriesSemaphores = new ConcurrentDictionary<Guid, SemaphoreSlim>();
        private readonly AggregatedDataRangeValidator _aggregatedDataRangeValidator = new AggregatedDataRangeValidator();

        public IngestionProcessor(IPersistentDataProvider persistence, IDataAggregator aggregator, IModelProvider model, IDataMessenger messenger, IRangeMerger<AggregatedDataRange> aggregatedMerger, IRangeMerger<TimeRange> timeMerger, IRangeFilterer<AggregatedDataRange> filterer, SeriesConfiguration configuration, ILogger<IngestionProcessor> logger)
        {
            _persistence = persistence;
            _aggregator = aggregator;
            _model = model;
            _messenger = messenger;
            _aggregatedMerger = aggregatedMerger;
            _timeMerger = timeMerger;
            _filterer = filterer;
            _configuration = configuration;
            _logger = logger;
            _telemetry.InstrumentationKey = "0def8f5e-9482-48ec-880d-4d2a81834a49";
        }

        public async Task ReceiveRealtimeRawData(Guid entity, Guid attribute, RawDataRange rawDataRange)
        {
            await _messenger.NotifyRaw(entity, attribute, rawDataRange);
        }

        public async Task ReceiveRealtimeAggregatedData(Guid entity, Guid attribute, AggregatedDataRange dataRange)
        {
            var affectedAggregatedPoints = await SaveHistoricalData(entity, attribute, dataRange);
            await _messenger.NotifyAggregated(entity, attribute, affectedAggregatedPoints);
        }

        public async Task ReceiveHistoricalData(Guid entity, Guid attribute, AggregatedDataRange dataRange)
        {
            await SaveHistoricalData(entity, attribute, dataRange);
        }

        private async Task<Dictionary<int, IEnumerable<AggregatedDataRange>>> SaveHistoricalData(Guid entity, Guid attribute, AggregatedDataRange dataRange)
        {
            _aggregatedDataRangeValidator.ValidateAndThrow(dataRange);

            var minAggregationSeconds = _configuration.AggregationsSeconds.Min();
            if (dataRange.AggregationSeconds != minAggregationSeconds)
            {
                throw new Exception($"Must be lowest aggregation {minAggregationSeconds}");
            }

            var series = await _model.ResolveSeries(entity, attribute, minAggregationSeconds);
            var maxAggregationSeconds = _configuration.AggregationsSeconds.Max();
            var quantised = dataRange.TimeRange.Quantise(maxAggregationSeconds);

            var semaphore = _seriesSemaphores.GetOrAdd(series, s => new SemaphoreSlim(1));
            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                //Console.WriteLine($"Writing data between {dataRange.TimeRange.Min} and {dataRange.TimeRange.Max}");

                var persisted = await _persistence.GetAggregatedData(entity, attribute, dataRange.AggregationSeconds, quantised);
                var existingTimeRanges = await _persistence.GetAllTimeRanges(entity, attribute);
                if (existingTimeRanges.Any(x => x.Intersects(dataRange.TimeRange)))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Overwriting data");
                    Console.ResetColor();
                }

                var merged = _aggregatedMerger.MergeRangeWithRanges(persisted, dataRange);
                var aggregations = _aggregator.Aggregate(merged, quantised, _configuration.AggregationsSeconds);
                //Console.WriteLine("Aggregations: " + JsonConvert.SerializeObject(aggregations));
                
                var affectedAggregatedPoints = aggregations.Select(x => FilterAggregationToAffectedRanges(x.Value, dataRange.TimeRange, x.Key)).Where(x => x != null && x.Any()).ToList();
                var rangesToPersist = affectedAggregatedPoints.Select(ranges => new ValueTuple<Guid, Guid, int, IEnumerable<AggregatedDataRange>>(entity, attribute, ranges.First().AggregationSeconds, ranges));
                await _persistence.SaveData(rangesToPersist);
                
                var afterTimeRanges = _timeMerger.MergeRangeWithRanges(existingTimeRanges, dataRange.TimeRange);
                await _persistence.SaveTimeRanges(entity, attribute, afterTimeRanges);

                var saved = await _persistence.GetTimeRanges(entity, attribute, new TimeRange(100, 4102444100));
                if (JsonConvert.SerializeObject(existingTimeRanges) == JsonConvert.SerializeObject(saved))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("No new time ranges saved");
                    Console.ResetColor();
                }

                /*Directory.CreateDirectory("timeRanges");
                File.WriteAllText($@"timeRanges\{entity}_{attribute}.json", JsonConvert.SerializeObject(afterTimeRanges));
                Console.WriteLine("Before time ranges: " + JsonConvert.SerializeObject(existingTimeRanges));
                Console.WriteLine("New time range: " + JsonConvert.SerializeObject(dataRange.TimeRange));
                Console.WriteLine("After time ranges: " + JsonConvert.SerializeObject(afterTimeRanges));*/

                _telemetry.TrackTrace($"Saved historical data from {dataRange.TimeRange.Min.ToDateTime():s} to {dataRange.TimeRange.Max.ToDateTime():s} after receiving {dataRange.Data.Count / 2} points from source");

                return affectedAggregatedPoints.ToDictionary(x => x.First().AggregationSeconds, x => x);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private IEnumerable<AggregatedDataRange> FilterAggregationToAffectedRanges(IEnumerable<AggregatedDataRange> dataRanges, TimeRange timeRange, int aggregationSeconds)
        {
            return _filterer.FilterRanges(dataRanges, timeRange.Quantise(aggregationSeconds));
            /*try
            {
                return filtered.SingleOrDefault();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("aggregationSeconds: " + JsonConvert.SerializeObject(aggregationSeconds));
                Console.WriteLine("timeRange: " + JsonConvert.SerializeObject(timeRange));
                Console.WriteLine("dataRanges: " + JsonConvert.SerializeObject(dataRanges));
                throw;
            }*/
        }
    }
}
