using UnityEngine;

public class Interaction : MonoBehaviour
{
    public bool Interacting { get; private set; }

    private bool _inRange = false;
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

            if (!Interacting && !_inRange)
            {
                _interactable = null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameManager.InteractableTag))
        {
            _inRange = true;
            _interactable = collision.GetComponent<IInteractable>();
            _interactable.ShowInteractable(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_interactable != null)
        {
            _inRange = false;
            _interactable.ShowInteractable(false);

            if (!Interacting)
            {
                _interactable = null;
            }
        }
    }
}
