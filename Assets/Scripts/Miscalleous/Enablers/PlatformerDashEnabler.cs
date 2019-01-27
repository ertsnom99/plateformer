using UnityEngine;

public class PlatformerDashEnabler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private PlatformerMovement m_movementScript;
    [SerializeField]
    private GameObject m_dashMeter;
    [SerializeField]
    private bool m_enable = true;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            m_movementScript.EnableDash(m_enable);
            m_dashMeter.SetActive(m_enable);
        }
    }
}
