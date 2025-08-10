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
        private Plane[] frustumPlanes;

        Camera ICameraService.Camera => Camera.main;

        private Camera Camera => ((ICameraService)this).Camera;

        [Inject]
        private void Construct(ICameraServiceMediator mediator)
        {
            this.mediator = mediator;

            frustumPlanes = new Plane[6];
        }

        bool ICameraService.IsVisible(Bounds bounds, out Plane outOfBoundsSide)
        {
            outOfBoundsSide = default;

            for (int i = 0; i < frustumPlanes.Length; i++)
            {
                if (!frustumPlanes[i].GetSide(bounds.center))
                {
                    outOfBoundsSide = frustumPlanes[i];
                    return false;
                }
            }

            return true;
            // return GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
        }

        void IUpdatable.Update()
        {
            GeometryUtility.CalculateFrustumPlanes(Camera, frustumPlanes);

            Transform[] targets = mediator.GetTargets();

            if (targets?.Length > 0)
            {
                cameraTarget.position = targets.Select(item => item?.position ?? Vector3.zero)
                    .Aggregate((vector1, vector2) => vector1 + vector2) / targets.Length;
            }
        }
    }
}
