using Infrastructure.Network.State;
using R3;

namespace Infrastructure.Network.Abstract
{
    public interface INetworkManager
    {
        bool IsServer { get; }
        ReadOnlyReactiveProperty<ConnectionState> ConnectionState { get; }
    }
}
