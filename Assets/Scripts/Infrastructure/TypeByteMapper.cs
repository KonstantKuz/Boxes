using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Infrastructure
{
    public static class TypeByteMapper
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<Type, byte> TypeToByteMap;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<byte, Type> ByteToTypeMap;
        // ReSharper disable once StaticMemberInGenericType
        private static byte _nextByteValue;

        static TypeByteMapper()
        {
            TypeToByteMap = new Dictionary<Type, byte>();
            ByteToTypeMap = new Dictionary<byte, Type>();
        }

        public static void RegisterTypes<T>()
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
            if (TypeToByteMap.ContainsKey(type))
            {
                Debug.LogWarning($"Type {type.Name} already registered.");
                return;
            }

            if (ByteToTypeMap.ContainsKey(_nextByteValue))
            {
                Debug.LogError($"Byte value {_nextByteValue} already assigned.");
                return;
            }

            TypeToByteMap[type] = _nextByteValue;
            ByteToTypeMap[_nextByteValue] = type;
            _nextByteValue++;

            if (_nextByteValue == 0)
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
            if (TypeToByteMap.TryGetValue(type, out byte byteValue))
            {
                return byteValue;
            }

            throw new ArgumentException($"Type {type.Name} not registered in the mapper.");
        }

        public static Type GetTypeFromByte(byte byteValue)
        {
            if (ByteToTypeMap.TryGetValue(byteValue, out Type type))
            {
                return type;
            }

            throw new ArgumentException($"Byte value {byteValue} does not correspond to any registered type.");
        }
    }
}
