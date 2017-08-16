using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Realtime
{
    public interface IWebsocketsReceiver
    {
        Task OnConnected(string socketId);
        Task OnDisconnected(string socketId);
        Task OnReceive(string socketId, string message);
    }
}