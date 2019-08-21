using UnityEngine;

public class Interaction : MonoBehaviour
{
    public bool Interacting { get; private set; }

    private IInteractable _interactable;

    private void Awake()
    {
        Interacting = false;
    }

    public void BeginInteraction()
    {
        if (_interactable != null)
        {
            Interacting = _interactable.BeginInteraction();
        }
    }

    public void StopInteraction()
    {
        if (_interactable != null)
        {
            Interacting = _interactable.StopInteraction();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameManager.InteractableTag))
        {
            _interactable = collision.GetComponent<IInteractable>();
            _interactable.ShowInteractable(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        _interactable.ShowInteractable(false);
        _interactable = null;
    }
}
