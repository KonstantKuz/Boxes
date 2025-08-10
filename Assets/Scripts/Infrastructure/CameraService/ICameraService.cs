using Infrastructure.Bootstrap;
using UnityEngine;

namespace Infrastructure.CameraService
{
    public interface ICameraService : IPostBuildInjectable
    {
        Camera Camera { get; }
        bool IsVisible(Bounds bounds, out Plane outOfBoundsSide);
    }
}
