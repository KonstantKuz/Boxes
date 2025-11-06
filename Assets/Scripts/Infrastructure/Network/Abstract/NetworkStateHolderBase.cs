using System;
using Mirror;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Infrastructure.Network.Abstract
{
    public class NetworkStateHolderBase : NetworkBehaviour
    {
        private readonly ReactiveProperty<byte[]> reactiveState = new();

        public INetworkSerializer Serializer;

        [HideInInspector]
        [SyncVar(hook = nameof(OnStateChanged))]
        public byte[] Data;

        [Inject]
        private void Construct(INetworkSerializer serializer)
        {
            Serializer = serializer;
        }

        public override void OnStartClient()
        {
            OnStateChanged(null, Data);
        }

        protected void WriteState<T>(T state) where T : INetworkState
        {
            byte[] serializedState = Serializer.Serialize(state);

            if (isServer)
            {
                Data = serializedState;
            }
            else
            {
                CmdWriteState(serializedState);
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdWriteState(byte[] serializedState)
        {
            Data = serializedState;
        }

        protected IDisposable Subscribe<T>(Action<T> callback) where T : INetworkState
        {
            return reactiveState
                .Where(bytes => bytes != null)
                .Subscribe(bytes => callback(Serializer.Deserialize<T>(bytes)));
        }

        private void OnStateChanged(byte[] oldState, byte[] newState)
        {
            reactiveState.Value = newState;
        }
    }
}
