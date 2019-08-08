using UnityEngine;

public class SpawnManager : MonoSingleton<SpawnManager>
{
    [Header("Spawn")]
    [SerializeField]
    private Transform _firstSpawn;

    public Vector3 SpawnPosition { get; private set; }

    protected override void Awake()
    {
        if (_firstSpawn)
        {
            SpawnPosition = _firstSpawn.position;
        }
        else
        {
            SpawnPosition = Vector3.zero;
        }
    }

    public void ChangeSpawnPosition(Vector3 spawnPosition)
    {
        SpawnPosition = spawnPosition;
    }
}
