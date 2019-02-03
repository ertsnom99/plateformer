using UnityEngine;

public class BossAmbiantEnabler : MonoBehaviour
{
    private bool m_triggered = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!m_triggered && col.CompareTag(GameManager.PlayerTag))
        {
            m_triggered = true;
            AmbiantManager.Instance.StartBossAmbiant();
        }
    }
}
