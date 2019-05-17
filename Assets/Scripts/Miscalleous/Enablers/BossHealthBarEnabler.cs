using UnityEngine;

public class BossHealthBarEnabler : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField]
    private GameObject _healthBar;
    [SerializeField]
    private bool _enable = true;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            _healthBar.SetActive(_enable);
        }
    }
}
