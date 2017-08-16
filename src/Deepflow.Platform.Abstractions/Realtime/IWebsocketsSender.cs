using System.Threading.Tasks;

namespace Deepflow.Platform.Abstractions.Realtime
{
    public interface IWebsocketsSender
    {
        Task Send(string socketId, string message);
    }
}