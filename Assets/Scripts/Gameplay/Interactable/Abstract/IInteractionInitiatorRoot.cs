namespace Gameplay.Interactable.Abstract
{
    public interface IInteractionInitiatorRoot
    {
        bool TryGetInteractionInitiator<T>(out T initiator);
    }
}
