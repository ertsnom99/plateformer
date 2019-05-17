using UnityEngine;

public class PlatformerWallJumpEnabler : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private PlatformerMovement _movementScript;
    [SerializeField]
    private bool _enable = true;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            _movementScript.EnableSlideOfWall(_enable);
            _movementScript.EnableWallJump(_enable);
        }
    }
}
