using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public struct EnemySpawnSetting
{
    public Explodable CurrentEnemie;
    public GameObject SpawnedEnemie;
    public Transform SpawnPosition;
    public CinemachineVirtualCamera VirtualCamera;
    public Transform Target;
    public float DistanceToDetect;
    public GameObject InfoUI;
    public float SpawnDelay;
}

public class EnemySpawnManager : MonoBehaviour, IProximityExplodableSubscriber
{
    [Header("Spawn")]
    [SerializeField]
    private EnemySpawnSetting[] _spawnSettings;

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
                StartCoroutine(SpawnEnemie(i));
            }
        }
    }

    private IEnumerator SpawnEnemie(int settingIndex)
    {
        yield return new WaitForSeconds(_spawnSettings[settingIndex].SpawnDelay);

        GameObject instanciatedEnemie = Instantiate(_spawnSettings[settingIndex].SpawnedEnemie, _spawnSettings[settingIndex].SpawnPosition.position, Quaternion.identity);
        PossessableCharacterController possessableCharacterControllerScript = instanciatedEnemie.GetComponent<PossessableCharacterController>();

        possessableCharacterControllerScript.SetInfoUI(_spawnSettings[settingIndex].InfoUI);
        possessableCharacterControllerScript.SetPossessionVirtualCamera(_spawnSettings[settingIndex].VirtualCamera);
        possessableCharacterControllerScript.SetTarget(_spawnSettings[settingIndex].Target);
        possessableCharacterControllerScript.SetDistanceToDetect(_spawnSettings[settingIndex].DistanceToDetect);

        _spawnSettings[settingIndex].VirtualCamera.Follow = instanciatedEnemie.transform;

        _spawnSettings[settingIndex].CurrentEnemie = instanciatedEnemie.GetComponent<Explodable>();
        _spawnSettings[settingIndex].CurrentEnemie.Subscribe(this);
    }

    public void EnableEnemieSpawn(bool spawnEnemie)
    {
        _spawnEnemies = spawnEnemie;
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
                if (_spawnSettings[i].CurrentEnemie.gameObject == explodableGameObject)
                {
                    StartCoroutine(SpawnEnemie(i));
                    break;
                }
            }
        }
    }
}
