using System;
using Infrastructure.Network.Abstract;

namespace Infrastructure.InteractionService.Abstract
{
    [Serializable]
    public abstract class InteractionContext : INetworkCommand
    {
    }
}
