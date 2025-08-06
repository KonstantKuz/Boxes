using R3;

namespace Infrastructure.Network.Abstract
{
    public interface INetworkFactory
    {
        ReactiveCommand<Unit> LocalSpawnStream { get; }
    }
}
