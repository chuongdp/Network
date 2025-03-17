using System.Threading.Tasks;
using UniRx;

namespace GameFoundation.Scripts.Network.Websocket
{
    public enum ServiceStatus
    {
        NotInitialize,
        Initialized,
        Connected,
        Closed
    }

    public interface IWebSocketService
    {
        ReactiveProperty<ServiceStatus> State { get; }

        Task OpenConnection();

        Task CloseConnection();
    }
}