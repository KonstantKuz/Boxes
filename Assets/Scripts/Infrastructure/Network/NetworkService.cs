using System;
using System.Collections.Generic;
using Infrastructure.Network.Abstract;
using MessagePack;
using Mirror;
using R3;
using Reflex.Attributes;

namespace Infrastructure.Network
{
    public class NetworkService : NetworkBehaviour, INetworkService
    {
        private ITypeMapper<byte> typeMapper;
        private readonly Dictionary<Type, List<Action<byte[]>>> executionObservers = new();
        private readonly Dictionary<Type, List<Action<byte[]>>> reactionObservers = new();

        [Inject]
        private void Construct(ITypeMapper<byte> typeMapper)
        {
            this.typeMapper = typeMapper;
        }

        byte[] INetworkSerializer.Serialize<T>(T data)
        {
            return MessagePackSerializer.Serialize(data);
        }

        T INetworkSerializer.Deserialize<T>(byte[] data)
        {
            return MessagePackSerializer.Deserialize<T>(data);
        }

        void INetworkService.SendCommand<T>(T command)
        {
            byte type =  typeMapper.GetKey<T>();
            byte[] payload = MessagePackSerializer.Serialize(command);
            CmdSendCommand(type, payload);
        }

        IDisposable INetworkService.ObserveToExecute<T>(Action<T> observer)
        {
            if (!executionObservers.TryGetValue(typeof(T), out List<Action<byte[]>> observers))
            {
                observers = new List<Action<byte[]>>();
                executionObservers[typeof(T)] = observers;
            }

            Action<byte[]> action = InvokeObserver(observer);

            observers.Add(action);

            return Disposable.Create(() => observers.Remove(action));
        }

        public IDisposable ObserveToReact<T>(Action<T> observer) where T : INetworkCommand
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

        [Command(requiresAuthority = false)]
        private void CmdSendCommand(byte type, byte[] command)
        {
            DispatchReceivedCommand(type, command);
            RpcDispatchReceivedCommand(type, command);
        }

        private void DispatchReceivedCommand(byte type, byte[] data)
        {
            if (executionObservers.TryGetValue(typeMapper.GetType<Type>(type), out List<Action<byte[]>> observers))
            {
                observers.ForEach(observer => observer(data));
            }
        }

        [ClientRpc]
        private void RpcDispatchReceivedCommand(byte type, byte[] data)
        {
            if (reactionObservers.TryGetValue(typeMapper.GetType<Type>(type), out List<Action<byte[]>> observers))
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
