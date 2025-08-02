using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Infrastructure.InteractionService
{
    // ReSharper disable once UnusedType.Global
    public static class InteractionTypeExtensions
    {
        private static readonly Dictionary<Type, Func<NetworkReader, InteractionContext>> readFunctions = new();

        public static void RegisterReader(Type type, Func<NetworkReader, InteractionContext> reader)
        {
            readFunctions.Add(type, reader);
        }

        // ReSharper disable once UnusedMember.Global
        public static InteractionContext CreateContext(this byte typeId, NetworkReader reader)
        {
            Type type = TypeByteMapper.GetTypeFromByte(typeId);

            if (readFunctions.TryGetValue(type, out Func<NetworkReader, InteractionContext> readFunc))
            {
                return readFunc.Invoke(reader);
            }

            Debug.LogError("No reader registered for type: " + type.Name);
            return null;
        }
    }
}
