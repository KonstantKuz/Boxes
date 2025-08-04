using System;
using System.Collections.Generic;
using MessagePack;
using Mirror;
using R3;
using UnityEngine;

namespace Infrastructure.Network
{
    public class NetworkService : NetworkBehaviour, INetworkService
    {
        private readonly Dictionary<Type, INetworkStateHolder> stateHolders = new();
        private readonly Dictionary<Type, List<Action<byte[]>>> stateObservers = new();
        private readonly Dictionary<Type, List<Action<byte[]>>> commandObservers = new();
        private readonly Dictionary<Type, List<Action<byte[]>>> reactionObservers = new();
        private readonly Dictionary<Type, IDisposable> stateDisposables = new();

        void INetworkService.RegisterStateHolder<T>(INetworkStateHolder stateHolder)
        {
            if (!stateHolders.TryAdd(typeof(T), stateHolder))
            {
                Debug.LogError($"Failed to register state holder of type {typeof(T)}.");
                return;
            }

            if (stateObservers.TryGetValue(typeof(T), out List<Action<byte[]>> observers))
            {
                IDisposable stateSubscription =
                    stateHolder.Subscribe(state => observers.ForEach(observer => observer.Invoke(state)));

                stateDisposables.Add(typeof(T), stateSubscription);
            }
        }

        void INetworkService.UnregisterStateHolder<T>(INetworkStateHolder stateHolder)
        {
            if (!stateHolders.Remove(typeof(T)))
            {
                Debug.LogWarning($"There is no state holder of type {typeof(T)}.");
                return;
            }

            if (stateDisposables.Remove(typeof(T), out IDisposable disposable))
            {
                disposable?.Dispose();
            }
        }

        byte[] INetworkService.Serialize<T>(T data)
        {
            return MessagePackSerializer.Serialize(data);
        }

        T INetworkService.ReadState<T>()
        {
            if (stateHolders.TryGetValue(typeof(T), out INetworkStateHolder stateHolder))
            {
                return MessagePackSerializer.Deserialize<T>(stateHolder.State);
            }

            return default;
        }

        void INetworkService.WriteState<T>(T state)
        {
            byte type = TypeByteMapper.GetByteFromType<T>();
            byte[] payload = MessagePackSerializer.Serialize(state);
            CmdWriteState(type, payload);
        }

        void INetworkService.SendCommand<T>(T command)
        {
            byte type =  TypeByteMapper.GetByteFromType<T>();
            byte[] payload = MessagePackSerializer.Serialize(command);
            CmdSendCommand(type, payload);
        }

        IDisposable INetworkService.ObserveCommand<T>(Action<T> observer)
        {
            if (!commandObservers.TryGetValue(typeof(T), out List<Action<byte[]>> observers))
            {
                observers = new List<Action<byte[]>>();
                commandObservers[typeof(T)] = observers;
            }

            Action<byte[]> action = InvokeObserver(observer);

            observers.Add(action);

            return Disposable.Create(() => observers.Remove(action));
        }

        public IDisposable ObserveReaction<T>(Action<T> observer) where T : INetworkCommand
        {
            if (!reactionObservers.TryGetValue(typeof(T), out List<Action<byte[]>> observers))
            {
                observers = new List<Action<byte[]>>();
                reactionObservers[typeof(T)] = observers;
            }

            Action<byte[]> action = InvokeObserver(observer);

            observers.Add(action);

            return Disposable.Create(() => observers.Remove(action));
        }

        IDisposable INetworkService.ObserveState<T>(Action<T> observer)
        {
            if (!stateObservers.TryGetValue(typeof(T), out List<Action<byte[]>> observers))
            {
                observers = new List<Action<byte[]>>();
                stateObservers[typeof(T)] = observers;
            }

            Action<byte[]> action = InvokeObserver(observer);

            observers.Add(action);

            if (stateHolders.TryGetValue(typeof(T), out INetworkStateHolder stateHolder))
            {
                action(stateHolder.State);
            }

            return Disposable.Create(() => observers.Remove(action));
        }

        [Command(requiresAuthority = false)]
        private void CmdWriteState(byte type, byte[] state)
        {
            Type mappedType = TypeByteMapper.GetTypeFromByte(type);
            stateHolders[mappedType].WriteState(state);
        }

        [Command(requiresAuthority = false)]
        private void CmdSendCommand(byte type, byte[] command)
        {
            DispatchReceivedCommand(type, command);
            RpcDispatchReceivedCommand(type, command);
        }

        private void DispatchReceivedCommand(byte type, byte[] data)
        {
            if (commandObservers.TryGetValue(TypeByteMapper.GetTypeFromByte(type), out List<Action<byte[]>> observers))
            {
                observers.ForEach(observer => observer(data));
            }
        }

        [ClientRpc]
        private void RpcDispatchReceivedCommand(byte type, byte[] data)
        {
            if (reactionObservers.TryGetValue(TypeByteMapper.GetTypeFromByte(type), out List<Action<byte[]>> observers))
            {
                observers.ForEach(observer => observer(data));
            }
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
