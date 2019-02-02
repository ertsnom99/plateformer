using System;
using UnityEngine;

[Serializable]
public struct EnemieSpawnSetting
{
    public ProximityExplodable currentEnemie;
    public GameObject spawnedEnemie;
    public Transform spawnPosition;
}

public class EnemieRespawner : MonoBehaviour, IProximityExplodableSubscriber
{
    [Header("Spawn")]
    [SerializeField]
    private EnemieSpawnSetting[] m_spawnSettings;
    [SerializeField]
    private Transform m_enemieTarget;

    private bool m_spawnEnemies = true;

    private void Start()
    {
        for (int i = 0; i < m_spawnSettings.Length; i++)
        {
            if (m_spawnSettings[i].currentEnemie != null)
            {
                m_spawnSettings[i].currentEnemie.Subscribe(this);
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
        if (m_spawnEnemies)
        {
            for (int i = 0; i < m_spawnSettings.Length; i++)
            {
                if (m_spawnSettings[i].currentEnemie.gameObject == explodableGameObject)
                {
                    SpawnEnemie(i);
                    break;
                }
            }
        }
    }

    private void SpawnEnemie(int settingIndex)
    {
        GameObject instanciatedEnemie = Instantiate(m_spawnSettings[settingIndex].spawnedEnemie, m_spawnSettings[settingIndex].spawnPosition.position, Quaternion.identity);
        instanciatedEnemie.GetComponent<AIControl>().SetTarget(m_enemieTarget);
        instanciatedEnemie.GetComponent<ProximityExplodable>().SetTarget(m_enemieTarget);

        m_spawnSettings[settingIndex].currentEnemie = instanciatedEnemie.GetComponent<ProximityExplodable>();
        m_spawnSettings[settingIndex].currentEnemie.Subscribe(this);
    }

    public void EnableEnemieSpawn(bool spawnEnemie)
    {
        m_spawnEnemies = spawnEnemie;
    }
}
