using UnityEngine;

public class PlatformerDashEnabler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private PlatformerMovement _movementScript;
    [SerializeField]
    private GameObject _dashMeter;
    [SerializeField]
    private bool _enable = true;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            _movementScript.EnableDash(_enable);
            _dashMeter.SetActive(_enable);
        }
    }
}
