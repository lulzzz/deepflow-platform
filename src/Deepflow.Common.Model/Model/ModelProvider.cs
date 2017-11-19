using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deepflow.Common.Model.Model
{
    public class ModelProvider : IModelProvider
    {
        private readonly Guid _dataSource = Guid.Parse("4055083b-c6be-4902-a209-7d2dba99abae");

        private readonly Dictionary<Guid, Dictionary<string, (Guid entity, Guid attribute)>> _dataSourceToModel;
        private readonly Dictionary<(Guid entity, Guid attribute, int aggregationSeconds), Guid> _modelToSeries;
        private readonly Dictionary<Guid, int> _seriesToAggregation;

        public ModelProvider(SeriesConfiguration configuration, ModelConfiguration model)
        {
            _dataSourceToModel = new Dictionary<Guid, Dictionary<string, (Guid entity, Guid attribute)>>
            {
                {_dataSource, model.Entities.SelectMany(entity => model.Attributes.Select(attribute => new ValueTuple<Guid, Guid>(entity, attribute))).ToDictionary(x => (x.Item1 + ":" + x.Item2).ToLower(), x => x)}
            };
                
            _modelToSeries = _dataSourceToModel.SelectMany(x => x.Value.Values.SelectMany(entityAttribute => configuration.AggregationsSeconds.Select(aggregation => new ValueTuple<Guid, Guid, int>(entityAttribute.entity, entityAttribute.attribute, aggregation)))).ToDictionary(x => x, x => GenerateSeriesGuid(x.Item1, x.Item2, x.Item3));
            _seriesToAggregation = _modelToSeries.ToDictionary(x => x.Value, x => x.Key.aggregationSeconds);
        }

        public Task<(Guid entity, Guid attribute)> ResolveEntityAndAttribute(Guid dataSource, string sourceName)
        {
            if (!_dataSourceToModel.TryGetValue(dataSource, out Dictionary<string, (Guid entity, Guid attribute)> names))
            {
                throw new Exception($"Can't find data source {dataSource}");
            }

            if (!names.TryGetValue(sourceName.ToLower(), out (Guid entity, Guid attribute) entityAttribute))
            {
                throw new Exception($"Can't find source name {sourceName}");
            }

            return Task.FromResult(entityAttribute);
        }

        public Task<Guid> ResolveSeries(Guid entity, Guid attribute, int aggregationSeconds)
        {
            if (!_modelToSeries.TryGetValue(new ValueTuple<Guid, Guid, int>(entity, attribute, aggregationSeconds), out Guid series))
            {
                throw new Exception($"Can't find entity attribute aggregation {entity}:{attribute}:{aggregationSeconds}");
            }

            return Task.FromResult(series);
        }

        public Task<int> ResolveAggregationForSeries(Guid series)
        {
            if (!_seriesToAggregation.TryGetValue(series, out int aggregationSeconds))
            {
                throw new Exception($"Can't find aggregation for series {series}");
            }

            return Task.FromResult(aggregationSeconds);
        }

        public Task<IEnumerable<string>> ResolveSourceNamesForDataSource(Guid dataSource)
        {
            if (!_dataSourceToModel.TryGetValue(dataSource, out Dictionary<string, (Guid entity, Guid attribute)> names))
            {
                throw new Exception($"Can't find data source {dataSource}");
            }

            return Task.FromResult(names.Select(x => x.Key.ToLower()));
        }

        public Guid GenerateSeriesGuid(Guid entity, Guid attribute, int aggregationSeconds)
        {
            return new Guid(entity.ToByteArray().Take(4).Concat(BitConverter.GetBytes(aggregationSeconds)).Concat(attribute.ToByteArray().Skip(8).Take(8)).ToArray());
        }
    }
}
