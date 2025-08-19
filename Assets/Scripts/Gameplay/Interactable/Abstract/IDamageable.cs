using R3;

namespace Gameplay.Interactable.Abstract
{
    public interface IDamageable
    {
        int MaxHitPoints { get; }
        ReadOnlyReactiveProperty<int> CurrentHitPoints { get; }
        void Initialize(int maxHitPoints);
        void TakeDamage(int hitPoints);
    }
}
