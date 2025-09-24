using R3;

namespace Gameplay.Interactable.Abstract
{
    public interface IDamageable
    {
        int MaxHitPoints { get; }
        ReadOnlyReactiveProperty<int> CurrentHitPoints { get; }
        void Initialize(int initialHitPoints = 0);
        void TakeDamage(int hitPoints);
    }
}
