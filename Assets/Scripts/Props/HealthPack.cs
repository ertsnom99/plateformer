using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private int m_healthGain = 100;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(GameManager.PlayerTag))
        {
            collision.GetComponent<Health>().Heal(m_healthGain);
            Destroy(gameObject);
        }
    }
}
