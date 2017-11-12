using System.Threading.Tasks;
using Deepflow.Platform.Abstractions.Realtime;

namespace Deepflow.Platform.Realtime
{
    public class NoopWebsocketsReceiver : IWebsocketsReceiver
    {
        public Task OnConnected(string socketId)
        {
            return Task.FromResult(0);
        }

        public Task OnDisconnected(string socketId)
        {
            return Task.FromResult(0);
        }

        public Task OnReceive(string socketId, string message)
        {
            return Task.FromResult(0);
        }

        public void SetSender(IWebsocketsSender sender)
        {
            
        }
    }
}
