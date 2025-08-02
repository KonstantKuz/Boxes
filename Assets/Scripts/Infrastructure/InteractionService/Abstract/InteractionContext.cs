using System;
using Mirror;

namespace Infrastructure.InteractionService.Abstract
{
    [Serializable]
    public abstract class InteractionContext
    {
        public abstract void Write(NetworkWriter writer);
    }
}
