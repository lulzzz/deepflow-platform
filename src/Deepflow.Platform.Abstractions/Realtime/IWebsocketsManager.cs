using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Realtime
{
    public interface IWebsocketsManager
    {
        Task HandleWebsocket(WebSocket socket, CancellationToken cancellationToken);
    }
}
