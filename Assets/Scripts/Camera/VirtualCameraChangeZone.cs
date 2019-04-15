using Cinemachine;
using UnityEngine;

public class VirtualCameraChangeZone : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField]
    private CinemachineVirtualCamera m_virtualCamera;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (VirtualCameraManager.Instance && m_virtualCamera && col.CompareTag(GameManager.PlayerTag))
        {
            VirtualCameraManager.Instance.ChangeVirtualCamera(m_virtualCamera);
        }
    }
}
