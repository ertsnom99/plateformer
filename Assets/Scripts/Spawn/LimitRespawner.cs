using UnityEngine;

public class LimitRespawner : MonoBehaviour
{
    [SerializeField]
    private Transform _respawnPosition;

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag(GameManager.PlayerTag))
        {
            other.transform.position = _respawnPosition.position;
        }
    }
}
