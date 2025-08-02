namespace Infrastructure.InteractionService.Abstract
{
    public interface IInteractable
    {
        InteractableState State { get; }
        void CmdInteract(InteractionContext interactionContext);
    }
}
