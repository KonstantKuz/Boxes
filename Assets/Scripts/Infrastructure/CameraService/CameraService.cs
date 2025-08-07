using System;
using System.Linq;
using Infrastructure.Bootstrap;
using Reflex.Attributes;
using UnityEngine;

namespace Infrastructure.CameraService
{
    [Serializable]
    public class CameraService : ICameraService, IUpdatable
    {
        [SerializeField]
        private Transform cameraTarget;

        private ICameraServiceMediator mediator;

        [Inject]
        private void Construct(ICameraServiceMediator mediator)
        {
            this.mediator = mediator;
        }

        public void Update()
        {
            Transform[] targets = mediator.GetTargets();

            if (targets?.Length > 0)
            {
                cameraTarget.position = targets.Select(item => item?.position ?? Vector3.zero)
                    .Aggregate((vector1, vector2) => vector1 + vector2) / targets.Length;
            }
        }
    }
}
