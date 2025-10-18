using System;
using UnityEngine;

namespace Infrastructure.World
{
    public interface IWorldObject
    {
        Guid Id { get; }
        GameObject Value { get; }
    }
}
