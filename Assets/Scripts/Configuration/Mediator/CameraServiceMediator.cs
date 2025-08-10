using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Bootstrap;
using Infrastructure.CameraService;
using Infrastructure.Network.Abstract;
using Infrastructure.Network.State;
using R3;
using Reflex.Attributes;
using UnityEngine;

namespace Configuration.Mediator
{
    [Serializable]
    public class CameraServiceMediator : ICameraServiceMediator, IInitializable
    {
        [SerializeField]
        private bool sharedMode;

        private INetworkFactory networkFactory;
        private INetworkStateHolder<ConnectionState> connectionStateHolder;

        private Transform[] soloTargets;
        private Transform[] sharedTargets;

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
            soloTargets = new []{networkFactory.LocalPlayer?.transform};

            sharedTargets = connectionStateHolder.GetState()?.Players
                .Select(netId => networkFactory.Spawned.GetValueOrDefault(netId)?.transform)
                .Where(item => item != null)
                .ToArray();
        }

        Transform[] ICameraServiceMediator.GetTargets()
        {
            return sharedMode ? sharedTargets : soloTargets;
        }
    }
}
