using System;
using System.Linq;
using System.Threading.Tasks;
using Deepflow.Common.Model.Model;
using Deepflow.Ingestion.Service.Processing;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Realtime.Messages;
using Deepflow.Platform.Abstractions.Realtime.Messages.Data;
using Deepflow.Platform.Common.Data.Persistence;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepflow.Ingestion.Service.Realtime
{
    public class RealtimeIngestionReceiver : IWebsocketsReceiver
    {
        private readonly IIngestionProcessor _processor;
        private readonly IModelProvider _model;
        private readonly IPersistentDataProvider _persistence;
        private readonly ILogger<RealtimeIngestionReceiver> _logger;
        private readonly TripCounterFactory _trip;
        private IWebsocketsSender _sender;
        private readonly AsyncCollection<(string socket, string message)> _queue = new AsyncCollection<(string socket, string message)>(null, 1000);

        public RealtimeIngestionReceiver(IIngestionProcessor processor, IModelProvider model, IPersistentDataProvider persistence, ILogger<RealtimeIngestionReceiver> logger, TripCounterFactory trip)
        {
            _processor = processor;
            _model = model;
            _persistence = persistence;
            _logger = logger;
            _trip = trip;

            Enumerable.Range(0, 10).ForEach(x => Task.Run(ProcessLoop));
        }

        public Task OnConnected(string socketId)
        {
            return Task.FromResult(0);
        }

        public Task OnDisconnected(string socketId)
        {
            return Task.FromResult(0);
        }

        public async Task OnReceive(string socketId, string messageString)
        {
            await _trip.Run("RealtimeIngestionReceiver.OnReceive", async () =>
            {
                await _queue.AddAsync(new ValueTuple<string, string>(socketId, messageString));
            });
        }

        private async Task ProcessLoop()
        {
            while (true)
            {
                (string socket, string message) item = new ValueTuple<string, string>();
                try
                {
                    item = await _queue.TakeAsync();
                    await _trip.Run("RealtimeIngestionReceiver.ProcessLoop", async () =>
                    {
                        var message = JsonConvert.DeserializeObject<IncomingMessage>(item.message, JsonSettings.Setttings);
                        if (message.MessageClass == IncomingMessageClass.Request)
                        {
                            var request = JsonConvert.DeserializeObject<RequestMessage>(item.message, JsonSettings.Setttings);
                            var response = await ReceiveRequest(request, item.message);
                            var responseText = JsonConvert.SerializeObject(response, JsonSettings.Setttings);
                            await _sender.Send(item.socket, responseText).ConfigureAwait(false);
                        }
                    });
                }
                catch (Exception exception)
                {
                    _logger.LogError(new EventId(109), exception, "Error processing request from queue");
                    await _queue.AddAsync(item);
                }
            }
        }

        private async Task<ResponseMessage> ReceiveRequest(RequestMessage request, string messageString)
        {
            /*if (request.RequestType == RequestType.AddAggregatedAttributeData)
            {
                var addRequest = JsonConvert.DeserializeObject<AddAggregatedAttributeDataRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveAddAttributeSubscriptionsRequest(addRequest);
            }
            if (request.RequestType == RequestType.AddAggregatedHistoricalAttributeData)
            {
                var addHistoricalRequest = JsonConvert.DeserializeObject<AddAggregatedAttributeHistoricalDataRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveAddAttributeHistoricalSubscriptionsRequest(addHistoricalRequest);
            }*/
            if (request.RequestType == RequestType.FetchAggregatedAttributeData)
            {
                var fetchRequest = JsonConvert.DeserializeObject<FetchAggregatedAttributeDataRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveFetchAggregatedAttributeDataRequest(fetchRequest).ConfigureAwait(false);
            }
            
            return null;
        }

        /*private async Task<AddAggregatedAttributeDataResponse> ReceiveAddAttributeSubscriptionsRequest(AddAggregatedAttributeDataRequest request)
        {
            var (entity, attribute) = await _model.ResolveEntityAndAttribute(request.DataSource, request.SourceName);
            await _processor.ReceiveRealtimeData(entity, attribute, request.AggregatedDataRange, request.RawDataRange);
            return new AddAggregatedAttributeDataResponse
            {
                ActionId = request.ActionId,
                MessageClass = OutgoingMessageClass.Response,
                ResponseType = ResponseType.AddAggregatedAttributeData,
                Succeeded = true
            };
        }

        private async Task<AddAggregatedAttributeHistoricalDataResponse> ReceiveAddAttributeHistoricalSubscriptionsRequest(AddAggregatedAttributeHistoricalDataRequest request)
        {
            var (entity, attribute) = await _model.ResolveEntityAndAttribute(request.DataSource, request.SourceName);
            await _processor.ReceiveHistoricalData(entity, attribute, request.AggregatedDataRange);
            return new AddAggregatedAttributeHistoricalDataResponse
            {
                ActionId = request.ActionId,
                MessageClass = OutgoingMessageClass.Response,
                ResponseType = ResponseType.AddAggregatedHistoricalAttributeData,
                Succeeded = true
            };
        }*/

        private async Task<FetchAggregatedAttributeDataResponse> ReceiveFetchAggregatedAttributeDataRequest(FetchAggregatedAttributeDataRequest request)
        {
            var dataRanges = await _persistence.GetData(request.EntityGuid, request.AttributeGuid, request.AggregationSeconds, request.TimeRange);
            return new FetchAggregatedAttributeDataResponse
            {
                ActionId = request.ActionId,
                MessageClass = OutgoingMessageClass.Response,
                ResponseType = ResponseType.FetchAggregatedAttributeData,
                Ranges = dataRanges,
                Succeeded = true
            };
        }

        public void SetSender(IWebsocketsSender sender)
        {
            _sender = sender;
        }
    }
}
