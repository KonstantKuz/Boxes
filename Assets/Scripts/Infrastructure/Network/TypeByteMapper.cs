using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Infrastructure.Network.Abstract;
using UnityEngine;

namespace Infrastructure.Network
{
    public class TypeByteMapper : ITypeMapper<byte>
    {
        private readonly Dictionary<Type, byte> typeToByteMap;
        private readonly Dictionary<byte, Type> byteToTypeMap;
        private byte nextByteValue;

        public TypeByteMapper()
        {
            typeToByteMap = new Dictionary<Type, byte>();
            byteToTypeMap = new Dictionary<byte, Type>();
        }

        public static TypeByteMapper Build(params Type[] types)
        {
            TypeByteMapper byteMapper = new TypeByteMapper();

            foreach (Type type in types)
            {
                byteMapper.RegisterInheritedTypes(type);
            }

            return byteMapper;
        }

        private void RegisterInheritedTypes(Type target)
        {
            IEnumerable<Type> types = Assembly
                .GetAssembly(target)
                .GetTypes()
                .OrderBy(type => type.FullName)
                .Where(type => target.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            foreach (Type type in types)
            {
                RegisterType(type);
            }
        }

        byte ITypeMapper<byte>.GetKey<T>()
        {
            Type type = typeof(T);
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

        Type ITypeMapper<byte>.GetType<T>(byte key)
        {
            if (byteToTypeMap.TryGetValue(key, out Type type))
            {
                return type;
            }

            throw new ArgumentException($"Byte value {key} does not correspond to any registered type.");
        }

        private void RegisterType(Type type)
        {
            if (typeToByteMap.ContainsKey(type))
            {
                Debug.LogWarning($"Type {type.Name} already registered.");
                return;
            }

            if (byteToTypeMap.ContainsKey(nextByteValue))
            {
                Debug.LogError($"Byte value {nextByteValue} already assigned.");
                return;
            }

            typeToByteMap[type] = nextByteValue;
            byteToTypeMap[nextByteValue] = type;
            nextByteValue++;

            if (nextByteValue == 0)
            {
                throw new OverflowException("Exceeded maximum byte value for type mapping.");
            }
        }
    }
}
