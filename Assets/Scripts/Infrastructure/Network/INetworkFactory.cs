using R3;

namespace Infrastructure.Network
{
    public interface INetworkFactory
    {
        ReactiveCommand<Unit> LocalSpawnStream { get; }
    }
}
