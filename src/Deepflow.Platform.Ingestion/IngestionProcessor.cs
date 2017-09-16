using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Ingestion;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Series;
using Orleans;

namespace Deepflow.Platform.Ingestion
{
    public class IngestionProcessor : IIngestionProcessor
    {
        private readonly IngestionConfiguration _configuration;

        public IngestionProcessor(IngestionConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task AddAggregatedRanges(Guid dataSource, string sourceName, int aggregationSeconds, IEnumerable<DataRange> dataRanges)
        {
            ISourceSeriesGrain series = GrainClient.GrainFactory.GetGrain<ISourceSeriesGrain>(SeriesIdHelper.ToSourceSeriesId(dataSource, sourceName));

            foreach (var dataRange in dataRanges)
            {
                var slices = dataRange.Chop(_configuration.MaxRangeSeconds);
                foreach (var slice in slices)
                {
                    await series.AddAggregatedData(slice, aggregationSeconds);
                }
            }
        }
    }
}
