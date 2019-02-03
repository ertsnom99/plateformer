using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private int m_healthGain = 100;

    private bool m_taked = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!m_taked && collision.CompareTag(GameManager.PlayerTag))
        {
            m_taked = true;

            collision.GetComponent<Health>().Heal(m_healthGain);
            Destroy(gameObject);
        }
    }
}
