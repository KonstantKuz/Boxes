using System;
using Infrastructure.Bootstrap;

namespace Infrastructure.Network.Abstract
{
    public interface INetworkService : IPostBuildInjectable
    {
        void SendCommand<T>(T command) where T : INetworkCommand;
        IDisposable ObserveToExecute<T>(Action<T> observer) where T : INetworkCommand;
        IDisposable ObserveToReact<T>(Action<T> observer) where T : INetworkCommand;
    }
}
