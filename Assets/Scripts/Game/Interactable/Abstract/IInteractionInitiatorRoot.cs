namespace Game.Interactable.Abstract
{
    public interface IInteractionInitiatorRoot
    {
        bool TryGetInteractionInitiator<T>(out T initiator);
    }
}
