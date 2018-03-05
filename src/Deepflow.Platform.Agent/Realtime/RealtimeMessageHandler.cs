using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;
using Deepflow.Platform.Abstractions.Realtime.Messages;
using Deepflow.Platform.Abstractions.Realtime.Messages.Data;
using Deepflow.Platform.Abstractions.Series;
using Deepflow.Platform.Agent.Provider;
using Deepflow.Platform.Core.Tools;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Deepflow.Platform.Agent.Realtime
{
    public class RealtimeMessageHandler : IWebsocketsReceiver, IDataSender
    {
        private readonly ILogger<RealtimeMessageHandler> _logger;
        private readonly ISourceDataProvider _data;
        private IWebsocketsSender _sender;

        public RealtimeMessageHandler(ILogger<RealtimeMessageHandler> logger, ISourceDataProvider data)
        {
            _logger = logger;
            _data = data;
        }

        public Task OnConnected(string socketId)
        {
            return Task.CompletedTask;
        }

        public Task OnDisconnected(string socketId)
        {
            return Task.CompletedTask;
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
            if (request.RequestType == RequestType.FetchRawSourceData)
            {
                var fetchRequest = JsonConvert.DeserializeObject<FetchRawSourceDataRequest>(messageString, JsonSettings.Setttings);
                return await ReceiveFetchRawSourceData(fetchRequest).ConfigureAwait(false);
            }
            return null;
        }

        private async Task<FetchRawSourceDataResponse> ReceiveFetchRawSourceData(FetchRawSourceDataRequest request)
        {
            var range = await _data.FetchRawDataWithEdges(request.SourceName, request.TimeRange);
            return new FetchRawSourceDataResponse
            {
                ActionId = request.ActionId,
                MessageClass = OutgoingMessageClass.Response,
                ResponseType = ResponseType.FetchAggregatedAttributeDataWithEdges,
                Range = range,
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
