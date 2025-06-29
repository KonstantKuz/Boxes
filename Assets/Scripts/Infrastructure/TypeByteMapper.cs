using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Infrastructure
{
    public static class TypeByteMapper<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<Type, byte> typeToByteMap;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<byte, Type> byteToTypeMap;
        // ReSharper disable once StaticMemberInGenericType
        private static byte nextByteValue;

        static TypeByteMapper()
        {
            typeToByteMap = new Dictionary<Type, byte>();
            byteToTypeMap = new Dictionary<byte, Type>();
        }

        public static void RegisterTypes()
        {
            IEnumerable<Type> types = Assembly
                .GetAssembly(typeof(T))
                .GetTypes()
                .Where(type => typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            foreach (Type type in types)
            {
                RegisterType(type);
            }
        }

        private static void RegisterType(Type type)
        {
            if (typeToByteMap.ContainsKey(type))
            {
                throw new InvalidOperationException($"Type {type.Name} already registered.");
            }

            if (byteToTypeMap.ContainsKey(nextByteValue))
            {
                throw new InvalidOperationException($"Byte value {nextByteValue} already assigned.");
            }

            typeToByteMap[type] = nextByteValue;
            byteToTypeMap[nextByteValue] = type;
            nextByteValue++;

            if (nextByteValue == 0)
            {
                throw new OverflowException("Exceeded maximum byte value for type mapping.");
            }
        }

        public static byte GetByteFromType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (typeToByteMap.TryGetValue(type, out byte byteValue))
            {
                return byteValue;
            }

            throw new ArgumentException($"Type {type.Name} not registered in the mapper.");
        }

        public static Type GetTypeFromByte(byte byteValue)
        {
            if (byteToTypeMap.TryGetValue(byteValue, out Type type))
            {
                return type;
            }

            throw new ArgumentException($"Byte value {byteValue} does not correspond to any registered type.");
        }
    }
}
