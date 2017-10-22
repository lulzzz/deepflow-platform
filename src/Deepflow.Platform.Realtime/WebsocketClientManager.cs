using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Realtime
{
    public class WebsocketClientManager
    {
        private readonly Uri _url;
        private readonly string _serverFriendlyName;
        private readonly int _retrySeconds;
        private readonly Func<Task> _onConnected;
        private readonly Func<string, Task> _onReceive;
        private readonly ILogger _logger;
        private ClientWebSocket _socket;
        
        public WebsocketClientManager(Uri url, string serverFriendlyName, int retrySeconds, Func<Task> onConnected, Func<string, Task> onReceive, ILogger logger)
        {
            _url = url;
            _serverFriendlyName = serverFriendlyName;
            _retrySeconds = retrySeconds;
            _onConnected = onConnected;
            _onReceive = onReceive;
            _logger = logger;
        }

        public async Task ListenForServer(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Trying to connect to {_serverFriendlyName} at {_url}...");

            var connected = false;
            while (!connected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _socket = new ClientWebSocket();
                    await _socket.ConnectAsync(_url, cancellationToken);
                    connected = true;
                    _logger.LogDebug($"Connected to {_serverFriendlyName}");
                }
                catch (Exception exception)
                {
                    _logger.LogDebug(new EventId(1000), exception, $"Unable to connect to {_serverFriendlyName} at {_url}, trying again in {_retrySeconds} seconds...");
                    await Task.Delay(_retrySeconds * 1000, cancellationToken);
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

#pragma warning disable 4014
            Task.Run(async () =>
#pragma warning restore 4014
            {
                while (_socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var messageString = await ReceiveStringAsync(_socket, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        await _onReceive(messageString);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogDebug(new EventId(1001), exception, $"Error receiving message from {_serverFriendlyName}");
                    }
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await ListenForServer(cancellationToken);
                }
            }, cancellationToken);

            await _onConnected();
        }

        public async Task SendMessage(string messageString)
        {
            var socket = _socket;
            if (socket != null)
            {
                _logger.LogDebug($"Sending realtime message...");
                await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageString)), WebSocketMessageType.Text, true, CancellationToken.None);
                _logger.LogDebug($"Sent realtime message");
            }
        }

        private static async Task<string> ReceiveStringAsync(WebSocket socket, CancellationToken cancellationToken)
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var memoryStream = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(buffer, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }
                    memoryStream.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                memoryStream.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }
}
