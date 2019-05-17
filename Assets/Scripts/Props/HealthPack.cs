using UnityEngine;

public class HealthPack : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private int _healthGain = 100;

    private bool _taked = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_taked && collision.CompareTag(GameManager.PlayerTag))
        {
            _taked = true;

            collision.GetComponent<Health>().Heal(_healthGain);
            Destroy(gameObject);
        }
    }
}
