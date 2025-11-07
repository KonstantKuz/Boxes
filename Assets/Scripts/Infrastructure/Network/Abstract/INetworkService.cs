using System;
using Infrastructure.Bootstrap;
using Mirror;

namespace Infrastructure.Network.Abstract
{
    public interface INetworkService : IPostBuildInjectable
    {
        void SendCommand<T>(T command) where T : INetworkCommand;
        IDisposable ObserveToExecute<T>(Action<T> observer) where T : INetworkCommand;
        IDisposable ObserveToReact<T>(Action<T> observer) where T : INetworkCommand;
        void AssignAuthority(NetworkIdentity target, uint? authorityId = null);
        void AssignAuthority(uint targetId, uint? authorityId = null);
    }
}
