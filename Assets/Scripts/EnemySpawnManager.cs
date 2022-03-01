using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public struct EnemySpawnSetting
{
    public GameObject CurrentEnemie;
    public Explodable ExplodableScript;
    public GameObject SpawnedEnemie;
    public Transform SpawnPosition;
    public CinemachineVirtualCamera VirtualCamera;
    public Transform Target;
    public float DistanceToDetect;
    public GameObject InfoUI;
    public float SpawnDelay;
    public bool Respawn;
}

public class EnemySpawnManager : MonoSingleton<EnemySpawnManager>, IProximityExplodableSubscriber
{
    [Header("Spawn")]
    [SerializeField]
    private EnemySpawnSetting[] _spawnSettings;

    private bool _spawnEnemies = true;

    private void Start()
    {
        for (int i = 0; i < _spawnSettings.Length; i++)
        {
            if (_spawnSettings[i].ExplodableScript != null)
            {
                _spawnSettings[i].ExplodableScript.Subscribe(this);
            }
            else if (_spawnSettings[i].Respawn)
            {
                StartCoroutine(SpawnEnemie(i));
            }
        }
    }

    private IEnumerator SpawnEnemie(int settingIndex)
    {
        yield return new WaitForSeconds(_spawnSettings[settingIndex].SpawnDelay);

        GameObject instanciatedEnemie = Instantiate(_spawnSettings[settingIndex].SpawnedEnemie, _spawnSettings[settingIndex].SpawnPosition.position, Quaternion.identity);
        PossessablePawn possessablePawn = instanciatedEnemie.GetComponent<PossessablePawn>();

        possessablePawn.SetPossessionVirtualCamera(_spawnSettings[settingIndex].VirtualCamera);

        _spawnSettings[settingIndex].VirtualCamera.Follow = instanciatedEnemie.transform;

        _spawnSettings[settingIndex].CurrentEnemie = instanciatedEnemie;
        _spawnSettings[settingIndex].ExplodableScript = instanciatedEnemie.GetComponent<Explodable>();

        if (_spawnSettings[settingIndex].ExplodableScript)
        {
            _spawnSettings[settingIndex].ExplodableScript.Subscribe(this);
        }
    }

    public void EnableEnemieSpawn(bool spawnEnemie)
    {
        _spawnEnemies = spawnEnemie;
    }

    public Vector3 GetSpawnPosition(GameObject enemie)
    {
        Vector3 spawnPosition = Vector3.zero;

        foreach(EnemySpawnSetting spawnSetting in _spawnSettings)
        {
            if (spawnSetting.SpawnedEnemie == enemie)
            {
                spawnPosition = spawnSetting.SpawnPosition.position;
            }
        }

        return spawnPosition;
    }

    // Methods of the IProximityExplodableSubscriber interface
    public void NotifyCountdownStarted(GameObject explodableGameObject, float timeRemaining) { }

    public void NotifyCountdownUpdated(GameObject explodableGameObject, float timeRemaining) { }

    public void NotifyCountdownFinished(GameObject explodableGameObject) { }

    public void NotifyExploded(GameObject explodableGameObject)
    {
        if (_spawnEnemies)
        {
            for (int i = 0; i < _spawnSettings.Length; i++)
            {
                if (_spawnSettings[i].CurrentEnemie == explodableGameObject && _spawnSettings[i].Respawn)
                {
                    StartCoroutine(SpawnEnemie(i));
                    break;
                }
            }
        }
    }
}
