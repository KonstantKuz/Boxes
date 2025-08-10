using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Infrastructure.Network.Abstract
{
    public interface INetworkFactory
    {
        ReactiveCommand<Unit> LocalSpawnStream { get; }
        GameObject LocalPlayer { get; }
        Dictionary<uint, GameObject> Spawned { get; }
    }
}
