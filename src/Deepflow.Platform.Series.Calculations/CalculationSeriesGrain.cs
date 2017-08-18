using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Orleans;

namespace Deepflow.Platform.Series.Calculations
{
    public class CalculationSeriesGrain : Grain, ICalculationSeriesGrain
    {
        private Guid _entity;
        private Guid _calculation;
        private Dictionary<int, Guid> _seriesGuids;
        private readonly ISeriesKnower _seriesKnower;
        private readonly IDataProvider _dataProvider;

        public CalculationSeriesGrain(ISeriesKnower seriesKnower, IDataProvider dataProvider)
        {
            _seriesKnower = seriesKnower;
            _dataProvider = dataProvider;
        }

        public override async Task OnActivateAsync()
        {
            var key = this.GetPrimaryKeyString();
            var parts = key.Split(':');
            _entity = Guid.Parse(parts[0]);
            _calculation = Guid.Parse(parts[1]);
            _seriesGuids = await _seriesKnower.GetCalculationSeriesGuids(_entity, _calculation);
            await base.OnActivateAsync();
        }

        public async Task<DataRange> GetData(TimeRange timeRange, int aggregationSeconds)
        {
            if (!_seriesGuids.TryGetValue(aggregationSeconds, out Guid seriesGuid))
            {
                throw new Exception($"Cannot find series GUID for calculation {_entity}:{_calculation}:{aggregationSeconds}");
            }
            return await _dataProvider.GetAttributeRange(seriesGuid, timeRange);
        }
    }
}
