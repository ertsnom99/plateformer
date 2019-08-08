using UnityEngine;

public class SpawnChanger : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField]
    private Transform _spawnLocation;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_spawnLocation && col.CompareTag(GameManager.PlayerTag))
        {
            SpawnManager.Instance.ChangeSpawnPosition(_spawnLocation.position);
        }
    }
}
