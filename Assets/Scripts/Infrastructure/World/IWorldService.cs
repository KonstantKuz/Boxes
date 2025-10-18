using System;
using Infrastructure.Bootstrap;

namespace Infrastructure.World
{
    public interface IWorldService : IPostBuildInjectable
    {
        void Register(IWorldObject worldObject);
        bool TryGetById(Guid id, out IWorldObject worldObject);
    }
}
