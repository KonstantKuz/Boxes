using UnityEngine;

namespace Infrastructure.WindowService.Abstract
{
    [CreateAssetMenu(
        fileName = nameof(WindowType),
        menuName = GlobalParams.Root + nameof(WindowType)
    )]
    public abstract class WindowType : ScriptableObject
    {
        public abstract string Id { get; }
    }
}
