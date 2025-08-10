using System;
using Infrastructure.Bootstrap;
using Infrastructure.CameraService;
using Infrastructure.InputService.Abstract;
using Infrastructure.InputService.Processors;
using Infrastructure.Network.Abstract;
using R3;
using Reflex.Attributes;

namespace Configuration.Mediator
{
    [Serializable]
    public class InputServiceMediator : IInputServiceMediator, IInitializable
    {
        private ICameraService cameraService;
        private INetworkFactory networkFactory;

        [Inject]
        private void Construct(ICameraService cameraService, INetworkFactory networkFactory)
        {
            this.cameraService = cameraService;
            this.networkFactory = networkFactory;
        }

        void IInitializable.Initialize()
        {
            networkFactory.LocalSpawnStream.Subscribe(UpdateMouseToWorldProcessor);
        }

        private void UpdateMouseToWorldProcessor(Unit _)
        {
            if (networkFactory.LocalPlayer)
            {
                MouseToWorldDirectionProcessor.SetParams(cameraService.Camera, networkFactory.LocalPlayer.transform);
            }
        }
    }
}
