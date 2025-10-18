using System.Collections.Generic;
using Mirror;
using R3;

namespace Infrastructure.Network.Abstract
{
    public interface INetworkFactory
    {
        ReactiveCommand<Unit> LocalSpawnStream { get; }
        NetworkIdentity LocalPlayer { get; }
        Dictionary<uint, NetworkIdentity> Players { get; }
        Dictionary<uint, NetworkIdentity> Spawned { get; }
    }
}
