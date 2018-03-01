using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Realtime
{
    public class WebsocketsManager : IWebsocketsManager, IWebsocketsSender
    {
        private readonly IWebsocketsReceiver _receiver;
        private readonly ILogger<WebsocketsManager> _logger;
        private static readonly ConcurrentDictionary<string, WebsocketWrapper> Sockets = new ConcurrentDictionary<string, WebsocketWrapper>();

        public WebsocketsManager(IWebsocketsReceiver receiver, ILogger<WebsocketsManager> logger)
        {
            _receiver = receiver;
            _logger = logger;
            _receiver.SetSender(this);
        }

        public async Task HandleWebsocket(WebSocket socket, CancellationToken cancellationToken)
        {
            var socketId = Guid.NewGuid().ToString();
            Sockets.TryAdd(socketId, new WebsocketWrapper { WebSocket = socket });
            await _receiver.OnConnected(socketId);

            try
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (socket.State != WebSocketState.Open)
                    {
                        break;
                    }

                    _logger.LogDebug($"Waiting to receive realtime message...");
                    var message = await ReceiveStringAsync(socket, cancellationToken);
                    if (!string.IsNullOrEmpty(message))
                    {
                        _logger.LogDebug($"Received realtime message...");
                        await _receiver.OnReceive(socketId, message);
                        _logger.LogDebug($"Realtime message complete");
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(new EventId(100), exception, "Error receving websockets message");
                throw;
            }
            finally
            {
                Sockets.TryRemove(socketId, out WebsocketWrapper dummy);
                await _receiver.OnDisconnected(socketId);
            }
        }

        private async Task<string> ReceiveStringAsync(WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result = null;
                do
                {
                    if (ct.IsCancellationRequested)
                    {
                        return null;
                    }

                    try
                    {
                        result = await socket.ReceiveAsync(buffer, ct);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                    }
                    catch (WebSocketException exception)
                    {
                        _logger.LogWarning($"Web socket exception, likely it was closed by the client: {exception.Message}");
                    }

                    if (ct.IsCancellationRequested)
                    {
                        return null;
                    }
                }
                while (result != null && !result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result == null || result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                // Encoding UTF8: https://tools.ietf.org/html/rfc6455#section-5.6
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public async Task Send(string socketId, string message)
        {
            if (!Sockets.TryGetValue(socketId, out WebsocketWrapper socket))
            {
                return;
            }

            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            await socket.Semaphore.WaitAsync();
            try
            {
                await socket.WebSocket.SendAsync(segment, WebSocketMessageType.Text, true, default(CancellationToken));
            }
            finally
            {
                socket.Semaphore.Release();
            }
        }

        private class WebsocketWrapper
        {
            public WebSocket WebSocket { get; set; }
            public SemaphoreSlim Semaphore { get; set; } = new SemaphoreSlim(1);
        }
    }
}
