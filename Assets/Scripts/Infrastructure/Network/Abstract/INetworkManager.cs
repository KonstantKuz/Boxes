using Infrastructure.Network.State;
using Mirror;
using R3;

namespace Infrastructure.Network.Abstract
{
    public interface INetworkManager
    {
        bool IsServer { get; }
        bool IsClientReady { get; }
        ReadOnlyReactiveProperty<ConnectionState> ConnectionState { get; }
        void AssignAuthority(NetworkIdentity target, uint? authorityId = null);
        void AssignAuthority(uint targetId, uint? authorityId = null);
    }
}
