using System;
using System.Collections.Generic;
using Infrastructure;
using MessagePack;
using Mirror;
using R3;
using UnityEngine;

namespace Network
{
    public class NetworkService : NetworkBehaviour, INetworkService
    {
        private readonly Dictionary<Type, INetworkStateHolder> _stateHolders = new();
        private readonly Dictionary<Type, List<Action<byte[]>>> _commandObservers = new();

        [ClientRpc]
        private void DispatchReceivedCommand(byte type, byte[] data)
        {
            if (_commandObservers.TryGetValue(TypeByteMapper.GetTypeFromByte(type), out List<Action<byte[]>> observers))
            {
                observers.ForEach(observer => observer(data));
            }
        }

        void INetworkService.RegisterStateHolder<T>(INetworkStateHolder stateHolder)
        {
            if (!_stateHolders.TryAdd(typeof(T), stateHolder))
            {
                Debug.LogError($"Failed to register state holder of type {typeof(T)}.");
            }
        }

        void INetworkService.UnregisterStateHolder<T>(INetworkStateHolder stateHolder)
        {
            if (!_stateHolders.Remove(typeof(T)))
            {
                Debug.LogWarning($"There is no state holder of type {typeof(T)}.");
            }
        }

        T INetworkService.ReadState<T>()
        {
            if (_stateHolders.TryGetValue(typeof(T), out INetworkStateHolder stateHolder))
            {
                return MessagePackSerializer.Deserialize<T>(stateHolder.State);
            }

            return default;
        }

        [Command(requiresAuthority = false)]
        void INetworkService.WriteState<T>(T state)
        {
            byte[] payload = MessagePackSerializer.Serialize(state);
            _stateHolders[typeof(T)].State = payload;
        }

        [Command(requiresAuthority = false)]
        void INetworkService.SendCommand<T>(T command)
        {
            byte type = TypeByteMapper.GetByteFromType(typeof(T));
            byte[] payload = MessagePackSerializer.Serialize(command);

            DispatchReceivedCommand(type, payload);
        }

        IDisposable INetworkService.ObserveCommand<T>(Action<T> observer)
        {
            if (!_commandObservers.TryGetValue(typeof(T), out List<Action<byte[]>> observers))
            {
                observers = new List<Action<byte[]>>();
            }

            observers.Add(InvokeObserver(observer));

            return Disposable.Create(() => observers.Remove(InvokeObserver(observer)));
        }

        IDisposable INetworkService.ObserveState<T>(Action<T> observer)
        {
            if (_stateHolders.TryGetValue(typeof(T), out INetworkStateHolder stateHolder))
            {
                return stateHolder.Subscribe(state => observer.Invoke(MessagePackSerializer.Deserialize<T>(state)));
            }

            Debug.LogError($"No state holder found for {typeof(T)} state.");
            return Disposable.Empty;
        }

        private static Action<byte[]> InvokeObserver<T>(Action<T> observer)
        {
            return data => InvokeObserver(observer, data);
        }

        private static void InvokeObserver<T>(Action<T> observer, byte[] bytes)
        {
            observer(MessagePackSerializer.Deserialize<T>(bytes));
        }
    }
}
