using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Orleans;

namespace Deepflow.Platform.Series.Attributes
{
    public class AttributeSeriesGrain : Grain, IAttributeSeriesGrain
    {
        private Guid _entity;
        private Guid _attribute;
        private Dictionary<int, Guid> _seriesGuids;
        private readonly ISeriesKnower _seriesKnower;
        private readonly IDataProvider _dataProvider;

        public AttributeSeriesGrain(ISeriesKnower seriesKnower, IDataProvider dataProvider)
        {
            _seriesKnower = seriesKnower;
            _dataProvider = dataProvider;
        }

        public override async Task OnActivateAsync()
        {
            var key = this.GetPrimaryKeyString();
            var parts = key.Split(':');
            _entity = Guid.Parse(parts[0]);
            _attribute = Guid.Parse(parts[1]);
            _seriesGuids = await _seriesKnower.GetAttributeSeriesGuids(_entity, _attribute);
            await base.OnActivateAsync();
        }

        public async Task<DataRange> GetData(TimeRange timeRange, int aggregationSeconds)
        {
            if (!_seriesGuids.TryGetValue(aggregationSeconds, out Guid seriesGuid))
            {
                throw new Exception($"Cannot find series GUID for attribute {_entity}:{_attribute}:{aggregationSeconds}");
            }
            return await _dataProvider.GetAttributeRange(seriesGuid, timeRange);
        }
    }
}
