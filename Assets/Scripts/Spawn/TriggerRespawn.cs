using UnityEngine;

public class TriggerRespawn : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            col.transform.position = SpawnManager.Instance.SpawnPosition;
        }
    }
}
