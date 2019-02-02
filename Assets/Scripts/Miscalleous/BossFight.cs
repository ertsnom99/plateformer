using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(GameManager))]

public class BossFight : MonoBehaviour, IHealthSubscriber
{
    [Header("Boss")]
    [SerializeField]
    private Health m_bossHealth;

    private GameManager m_gameManager;

    private void Awake()
    {
        m_gameManager = GetComponent<GameManager>();

        m_bossHealth.Subscribe(this);
    }

    // Methods of the IHealable interface
    public void NotifyDamageApplied(Health healthScript, int damage) { }

    public void NotifyHealthChange(Health healthScript, int health) { }

    public void NotifyHealthDepleted(Health healthScript)
    {
        m_gameManager.EndGame(true);
    }

    public void NotifyJustSubscribed(Health healthScript) { }
}
