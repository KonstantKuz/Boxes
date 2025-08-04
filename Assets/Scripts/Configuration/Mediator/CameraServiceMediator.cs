using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Bootstrap;
using Infrastructure.CameraService;
using Infrastructure.Network;
using Mirror;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.Mediator
{
    [Serializable]
    public class CameraServiceMediator : ICameraServiceMediator, IInitializable
    {
        private INetworkService networkService;
        private INetworkFactory networkFactory;
        private ConnectionState connectionState;

        private Transform[] targets;

        [Inject]
        private void Construct(INetworkService networkService, INetworkFactory networkFactory)
        {
            this.networkService = networkService;
            this.networkFactory = networkFactory;
        }

        void IInitializable.Initialize()
        {
            networkService.ObserveState<ConnectionState>(OnConnectionStateChanged);

            networkFactory.LocalSpawnStream.Subscribe(OnLocalSpawnStream);
        }

        private void OnLocalSpawnStream(Unit _)
        {
            UpdateCameraTarget();
        }

        private void OnConnectionStateChanged(ConnectionState connectionState)
        {
            this.connectionState = connectionState;
            UpdateCameraTarget();
        }

        private void UpdateCameraTarget()
        {
            targets = connectionState?.Players
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
