using UnityEngine;

namespace Infrastructure.WindowService.Abstract
{
    [CreateAssetMenu(fileName = "WindowType", menuName = "WindowType", order = 0)]
    public abstract class WindowType : ScriptableObject
    {
        public abstract string Id { get; }
    }
}
