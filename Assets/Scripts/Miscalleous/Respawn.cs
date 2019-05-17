using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField]
    private Transform _respawnPos;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_respawnPos && col.CompareTag(GameManager.PlayerTag))
        {
            col.transform.position = _respawnPos.position;
        }
    }
}
