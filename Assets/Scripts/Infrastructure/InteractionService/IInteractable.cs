namespace Infrastructure.InteractionService
{
    public interface IInteractable
    {
        InteractableState State { get; }
        void CmdInteract(InteractionContext interactionContext);
    }
}
