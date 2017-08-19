using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Deepflow.Platform.Realtime
{
    public class WebsocketsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebsocketsManager _manager;

        public WebsocketsMiddleware(RequestDelegate next, IWebsocketsManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest || context.Request.Path != "/ws")
            {
                await _next.Invoke(context);
                return;
            }

            CancellationToken cancellationToken = context.RequestAborted;
            WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();

            await _manager.HandleWebsocket(socket, cancellationToken);

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
            socket.Dispose();
        }
    }

    public static class WebsocketsMiddlewareExtensions
    {
        public static void UseWebSocketsHandler(this IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.UseMiddleware<WebsocketsMiddleware>();
        }
    }
}
