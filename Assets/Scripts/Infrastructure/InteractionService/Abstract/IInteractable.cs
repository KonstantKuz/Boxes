namespace Infrastructure.InteractionService.Abstract
{
    public interface IInteractable
    {
        InteractableState State { get; }
        void Interact(InteractionContext interactionContext);
    }
}
