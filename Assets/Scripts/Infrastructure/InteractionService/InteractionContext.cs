using System;
using Mirror;

namespace Infrastructure.InteractionService
{
    [Serializable]
    public abstract class InteractionContext
    {
        public abstract void Write(NetworkWriter writer);
    }
}
