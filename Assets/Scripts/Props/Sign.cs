using UnityEngine;

public class Sign : MonoBehaviour, IInteractable
{
    [Header("Interact")]
    [SerializeField]
    private GameObject _interactableDisplay;
    [Header("UI")]
    [SerializeField]
    private GameObject _infoUI;

    private void Awake()
    {
        _infoUI.SetActive(false);
        ShowInteractable(false);
    }

    // Methods of the IInteractable interface
    public void ShowInteractable(bool show)
    {
        _interactableDisplay.SetActive(show);
    }

    public bool BeginInteraction()
    {
        _infoUI.SetActive(!_infoUI.activeSelf);
        return _infoUI.activeSelf;
    }

    public bool StopInteraction()
    {
        return _infoUI.activeSelf;
    }
}
