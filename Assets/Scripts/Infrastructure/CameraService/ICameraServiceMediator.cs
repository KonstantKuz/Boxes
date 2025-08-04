using Infrastructure.Bootstrap;
using UnityEngine;

namespace Infrastructure.CameraService
{
    public interface ICameraServiceMediator : IPostBuildInjectable
    {
        Transform[] GetTargets();
    }
}
