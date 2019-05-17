using Cinemachine;
using UnityEngine;

public class VirtualCameraChangeZone : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField]
    private CinemachineVirtualCamera _virtualCamera;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (VirtualCameraManager.Instance && _virtualCamera && col.CompareTag(GameManager.PlayerTag))
        {
            VirtualCameraManager.Instance.ChangeVirtualCamera(_virtualCamera);
        }
    }
}
