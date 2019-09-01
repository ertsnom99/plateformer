using UnityEngine;

public class ForcedInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField]
    private bool _onlyFirstInteraction = false;

    private bool _triggeredInteraction = false;

    private void Awake()
    {
        IInteractable interactable = GetComponent<IInteractable>();

        if (interactable == null)
        {
            Debug.LogError(gameObject.name + " doesn't have an  IInteractable script, but it has a " + GetType() + " script!");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!_triggeredInteraction && collision.CompareTag(GameManager.PlayerTag))
        {
            Interaction interactionScript = collision.GetComponent<Interaction>();
            interactionScript.BeginInteraction();

            if (interactionScript.Interacting)
            {
                _triggeredInteraction = true;
                interactionScript.StopInteraction();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_onlyFirstInteraction && _triggeredInteraction && collision.CompareTag(GameManager.PlayerTag))
        {
            _triggeredInteraction = false;
        }
    }
}
