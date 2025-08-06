using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Bootstrap;
using Infrastructure.CameraService;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using Mirror;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.Mediator
{
    [Serializable]
    public class CameraServiceMediator : ICameraServiceMediator, IInitializable
    {
        private INetworkFactory networkFactory;
        private INetworkStateHolder<ConnectionState> connectionStateHolder;

        private Transform[] targets;

        [Inject]
        private void Construct(
            INetworkFactory networkFactory,
            INetworkStateHolder<ConnectionState> connectionStateHolder
        )
        {
            this.networkFactory = networkFactory;
            this.connectionStateHolder = connectionStateHolder;
        }

        void IInitializable.Initialize()
        {
            connectionStateHolder.Subscribe(OnConnectionStateChanged);

            networkFactory.LocalSpawnStream.Subscribe(OnLocalSpawnStream);
        }

        private void OnLocalSpawnStream(Unit _)
        {
            UpdateCameraTarget();
        }

        private void OnConnectionStateChanged(ConnectionState _)
        {
            UpdateCameraTarget();
        }

        private void UpdateCameraTarget()
        {
            targets = connectionStateHolder.State?.Players
                .Select(netId => NetworkClient.spawned.GetValueOrDefault(netId)?.transform)
                .Where(item => item != null)
                .ToArray();
        }

        Transform[] ICameraServiceMediator.GetTargets()
        {
            return targets;
        }
    }
}
