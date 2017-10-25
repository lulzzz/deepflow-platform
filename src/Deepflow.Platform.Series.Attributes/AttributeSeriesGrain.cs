/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Abstractions.Series.Attribute;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace Deepflow.Platform.Series.Attributes
{
    [Reentrant]
    public class AttributeSeriesGrain : Grain, IAttributeSeriesGrain
    {
        private Guid _entity;
        private Guid _attribute;
        private readonly IAttributeDataProviderFactory _providerFactory;
        private IAttributeDataProvider _provider;
        private readonly ILogger<AttributeSeriesGrain> _logger;
        private readonly TripCounterFactory _tripCounterFactory;
        private readonly ObserverSubscriptionManager<ISeriesObserver> _subscriptions = new ObserverSubscriptionManager<ISeriesObserver>();

        public AttributeSeriesGrain(IAttributeDataProviderFactory providerFactory, ILogger<AttributeSeriesGrain> logger, TripCounterFactory tripCounterFactory)
        {
            _providerFactory = providerFactory;
            _logger = logger;
            _tripCounterFactory = tripCounterFactory;
        }

        public override async Task OnActivateAsync()
        {
            var key = this.GetPrimaryKeyString();
            var parts = key.Split(':');
            _entity = Guid.Parse(parts[0]);
            _attribute = Guid.Parse(parts[1]);

            _provider = _providerFactory.Create(_entity, _attribute);

            await base.OnActivateAsync();
        }

        public async Task<IEnumerable<AggregatedDataRange>> GetAggregatedData(TimeRange timeRange, int aggregationSeconds)
        {
            return await _provider.GetData(timeRange, aggregationSeconds);
        }

        public async Task ReceiveData(AggregatedDataRange aggregatedRanges)
        {
            try
            {
                await _tripCounterFactory.Run("AttributeSeriesGrain.ReceiveData", async () =>
                {
                    var affectedAggregations = await _provider.AddData(aggregatedRanges);

                    // Notify subscribers with affected data
                    var notifyAggregations = affectedAggregations.ToDictionary(x => x.AggregationSeconds, x => x);
                    _subscriptions.Notify(observer => observer.ReceiveData(_entity, _attribute, notifyAggregations));
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(new EventId(103), exception, "Error when adding aggregated data");
                throw;
            }
        }

        public Task Subscribe(ISeriesObserver observer)
        {
            _subscriptions.Subscribe(observer);
            return Task.FromResult(0);
        }

        public Task Unsubscribe(ISeriesObserver observer)
        {
            try
            {
                _subscriptions.Unsubscribe(observer);
            }
            catch (OrleansException exception)
            {
                _logger.LogError(new EventId(104), exception, "Unsubscribed already");
            }
            return Task.FromResult(0);
        }
    }
}
*/
