public interface IInteractable
{
    void ShowInteractable(bool show);
    // Returns if the interactable is being interacted with
    bool BeginInteraction();
    bool StopInteraction();
}
