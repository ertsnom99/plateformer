using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField]
    private PlayerController _playerController;

    [Header("UI")]
    [SerializeField]
    private GameObject _buttonToFocus;
    [SerializeField]
    private RectTransform _infosTransform;

    public void Show(bool show)
    {
        if (show)
        {
            // Clear infos child
            if (_infosTransform.childCount > 0)
            {
                foreach (Transform child in _infosTransform)
                {
                    Destroy(child.gameObject);
                }
            }

            // Get infos HUD and add it
            Pawn playerControlledPawn = _playerController.GetControlledPawn();

            if (!playerControlledPawn)
            {
                Debug.Log("Possessing nothing!");
            }
            else
            {
                PawnInfos pawnInfos = playerControlledPawn.gameObject.GetComponent<PawnInfos>();

                if (pawnInfos)
                {
                    Instantiate(pawnInfos.InfosHUD, _infosTransform);
                }
                else
                {
                    Debug.Log("No infos available!");
                }
            }
        }

        gameObject.SetActive(show);

        if (show)
        {
            // Focus on first button
            StartCoroutine(FocusButtonLater());
        }
    }

    private IEnumerator FocusButtonLater()
    {
        yield return null;

        // Focus on first button
        EventSystem eventSystem = EventSystem.current;
        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(_buttonToFocus);
    }
}
