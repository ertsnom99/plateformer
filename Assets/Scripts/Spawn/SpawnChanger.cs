using UnityEngine;

public class SpawnChanger : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField]
    private Transform _spawnLocation;

    [Header("Activation")]
    [SerializeField]
    private bool m_enemyActivateSpawnChanger = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (_spawnLocation && col.CompareTag(GameManager.PlayerTag) || (m_enemyActivateSpawnChanger && col.CompareTag(GameManager.EnemyTag) && col.GetComponent<PossessablePawn>().IsPossessed))
        {
            SpawnManager.Instance.ChangeSpawnPosition(_spawnLocation.position);
        }
    }
}
