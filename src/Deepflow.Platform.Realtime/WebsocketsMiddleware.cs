using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Deepflow.Platform.Realtime
{
    public class WebsocketsMiddleware
    {
        private readonly ILogger<WebsocketsMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IWebsocketsManager _manager;
        private readonly string _path;

        public WebsocketsMiddleware(ILogger<WebsocketsMiddleware> logger, RequestDelegate next, IWebsocketsManager manager, string path)
        {
            _logger = logger;
            _next = next;
            _manager = manager;
            _path = path;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest || context.Request.Path != _path)
            {
                await _next.Invoke(context);
                return;
            }

            CancellationToken cancellationToken = context.RequestAborted;
            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();

            try
            {
                await _manager.HandleWebsocket(socket, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(new EventId(102), exception, "Error with websockets request");
            }

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
            socket.Dispose();
        }
    }

    public static class WebsocketsMiddlewareExtensions
    {
        public static void UseWebSocketsHandler(this IApplicationBuilder app, string path)
        {
            app.UseWebSockets();
            app.UseMiddleware<WebsocketsMiddleware>(path);
        }
    }
}
