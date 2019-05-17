using System;
using UnityEngine;

[Serializable]
public struct EnemieSpawnSetting
{
    public ProximityExplodable CurrentEnemie;
    public GameObject SpawnedEnemie;
    public Transform SpawnPosition;
}

public class EnemieRespawner : MonoBehaviour, IProximityExplodableSubscriber
{
    [Header("Spawn")]
    [SerializeField]
    private EnemieSpawnSetting[] _spawnSettings;
    [SerializeField]
    private Transform _enemieTarget;

    private bool _spawnEnemies = true;

    private void Start()
    {
        for (int i = 0; i < _spawnSettings.Length; i++)
        {
            if (_spawnSettings[i].CurrentEnemie != null)
            {
                _spawnSettings[i].CurrentEnemie.Subscribe(this);
            }
            else
            {
                SpawnEnemie(i);
            }
        }
    }

    // Methods of the IProximityExplodableSubscriber interface
    public void NotifyCountdownStarted(GameObject explodableGameObject, float timeRemaining) { }

    public void NotifyCountdownUpdated(GameObject explodableGameObject, float timeRemaining) { }

    public void NotifyCountdownFinished(GameObject explodableGameObject)
    {
        if (_spawnEnemies)
        {
            for (int i = 0; i < _spawnSettings.Length; i++)
            {
                if (_spawnSettings[i].CurrentEnemie.gameObject == explodableGameObject)
                {
                    SpawnEnemie(i);
                    break;
                }
            }
        }
    }

    private void SpawnEnemie(int settingIndex)
    {
        GameObject instanciatedEnemie = Instantiate(_spawnSettings[settingIndex].SpawnedEnemie, _spawnSettings[settingIndex].SpawnPosition.position, Quaternion.identity);
        instanciatedEnemie.GetComponent<AIControl>().SetTarget(_enemieTarget);
        instanciatedEnemie.GetComponent<ProximityExplodable>().SetTarget(_enemieTarget);

        _spawnSettings[settingIndex].CurrentEnemie = instanciatedEnemie.GetComponent<ProximityExplodable>();
        _spawnSettings[settingIndex].CurrentEnemie.Subscribe(this);
    }

    public void EnableEnemieSpawn(bool spawnEnemie)
    {
        _spawnEnemies = spawnEnemie;
    }
}
