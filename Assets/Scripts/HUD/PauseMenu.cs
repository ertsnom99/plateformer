using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private PlayerController _playerController;

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
    }
}
