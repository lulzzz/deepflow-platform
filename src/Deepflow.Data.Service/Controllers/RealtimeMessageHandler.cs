using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Common.Model.Model;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Realtime.Messages;
using Deepflow.Platform.Abstractions.Realtime.Messages.Data;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Common.Data.Persistence;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepflow.Platform.Controllers
{
    public class RealtimeMessageHandler : IWebsocketsReceiver, IDataSender
    {
        private readonly ILogger<RealtimeMessageHandler> _logger;
        private readonly IPersistentDataProvider _persistence;
        private readonly IModelProvider _model;
        private IWebsocketsSender _sender;

        public RealtimeMessageHandler(ILogger<RealtimeMessageHandler> logger, IPersistentDataProvider persistence, IModelProvider model)
        {
            _logger = logger;
            _persistence = persistence;
            _model = model;
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
            var message = JsonConvert.DeserializeObject<IncomingMessage>(messageString, JsonSettings.Setttings);
            if (message.MessageClass == IncomingMessageClass.Request)
            {
                var request = JsonConvert.DeserializeObject<RequestMessage>(messageString, JsonSettings.Setttings);
                var response = await ReceiveRequest(request, messageString);
                var responseText = JsonConvert.SerializeObject(response, JsonSettings.Setttings);
                await _sender.Send(socketId, responseText).ConfigureAwait(false);
            }
        }

        private async Task<ResponseMessage> ReceiveRequest(RequestMessage request, string messageString)
        {
            if (request.RequestType == RequestType.FetchAggregatedAttributeDataWithEdges)
            {
                var fetchRequest = JsonConvert.DeserializeObject<FetchAggregatedAttributeDataWithEdgesRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveFetchAggregatedAttributeDataRequest(fetchRequest).ConfigureAwait(false);
            }
            return null;
        }

        private async Task<FetchAggregatedAttributeDataWithEdgesResponse> ReceiveFetchAggregatedAttributeDataRequest(FetchAggregatedAttributeDataWithEdgesRequest request)
        {
            var dataRanges = await _persistence.GetAggregatedDataWithEdges(request.EntityGuid, request.AttributeGuid, request.AggregationSeconds, request.TimeRange);
            return new FetchAggregatedAttributeDataWithEdgesResponse
            {
                ActionId = request.ActionId,
                MessageClass = OutgoingMessageClass.Response,
                ResponseType = ResponseType.FetchAggregatedAttributeDataWithEdges,
                Ranges = dataRanges,
                Succeeded = true
            };
        }

        public void SetSender(IWebsocketsSender sender)
        {
            _sender = sender;
        }

        public void SendData(string socketId, Guid entity, Guid attribute, Dictionary<int, AggregatedDataRange> aggregatedRanges)
        {
            throw new NotImplementedException();
        }
    }
}
