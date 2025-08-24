using System;
using UnityEngine;

namespace Infrastructure.World
{
    public interface IWorldObject
    {
        Guid Id { get; }
        Guid TypeId { get; }
        GameObject Value { get; }
    }
}
