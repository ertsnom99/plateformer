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


    private void Start()
    {
        foreach (EnemieSpawnSetting setting in m_spawnSettings)
        {
            if (setting.currentEnemie != null)
            {
                setting.currentEnemie.Subscribe(this);
            }
        }
    }

    // Methods of the IProximityExplodableSubscriber interface
    public void NotifyCountdownStarted(GameObject explodableGameObject, float timeRemaining) { }

    public void NotifyCountdownUpdated(GameObject explodableGameObject, float timeRemaining) { }

    public void NotifyCountdownFinished(GameObject explodableGameObject)
    {
        int settingIndex = -1;

        for (int i = 0; i < m_spawnSettings.Length; i++)
        {
            if (m_spawnSettings[i].currentEnemie.gameObject == explodableGameObject)
            {
                settingIndex = i;
                break;
            }
        }

        if (settingIndex != -1)
        {
            GameObject instanciatedEnemie = Instantiate(m_spawnSettings[settingIndex].spawnedEnemie, m_spawnSettings[settingIndex].spawnPosition.position, Quaternion.identity);
            instanciatedEnemie.GetComponent<AIProximityActivator>().SetTarget(m_enemieTarget);
            instanciatedEnemie.GetComponent<AIControl>().SetTarget(m_enemieTarget);
            instanciatedEnemie.GetComponent<ProximityExplodable>().SetTarget(m_enemieTarget);

            m_spawnSettings[settingIndex].currentEnemie = instanciatedEnemie.GetComponent<ProximityExplodable>();
            m_spawnSettings[settingIndex].currentEnemie.Subscribe(this);
        }
    }
}
