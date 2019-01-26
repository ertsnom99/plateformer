using Cinemachine;
using UnityEngine;

public class VirtualCameraChangeZone : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField]
    private VirtualCameraManager m_virtualCameraManager;
    [SerializeField]
    private CinemachineVirtualCamera m_virtualCamera;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (m_virtualCameraManager && m_virtualCamera && col.CompareTag(GameManager.PlayerTag))
        {
            m_virtualCameraManager.ChangeVirtualCamera(m_virtualCamera);
        }
    }
}
