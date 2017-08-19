using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;

namespace Deepflow.Platform.Realtime
{
    public class WebsocketsManager : IWebsocketsManager, IWebsocketsSender
    {
        private readonly IWebsocketsReceiver _receiver;
        private static readonly ConcurrentDictionary<string, WebSocket> Sockets = new ConcurrentDictionary<string, WebSocket>();

        public WebsocketsManager(IWebsocketsReceiver receiver)
        {
            _receiver = receiver;
            _receiver.SetSender(this);
        }

        public async Task HandleWebsocket(WebSocket socket, CancellationToken cancellationToken)
        {
            var socketId = Guid.NewGuid().ToString();
            Sockets.TryAdd(socketId, socket);

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

                    var message = await ReceiveStringAsync(socket, cancellationToken);
                    if (!string.IsNullOrEmpty(message))
                    {
                        await _receiver.OnReceive(socketId, message).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                WebSocket dummy;
                Sockets.TryRemove(socketId, out dummy);
            }
        }

        private static async Task<string> ReceiveStringAsync(WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            var buffer = new ArraySegment<byte>(new byte[8192]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    ct.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, ct);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
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
            if (!Sockets.TryGetValue(socketId, out WebSocket socket))
            {
                return;
            }

            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);
            await socket.SendAsync(segment, WebSocketMessageType.Text, true, default(CancellationToken));
        }
    }
}
