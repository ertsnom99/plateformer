using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour, IHealthSubscriber
{
    [SerializeField]
    private Health m_playerHealth;

    private Slider m_healthBar;

    private void Awake()
    {
        m_healthBar = GetComponentInChildren<Slider>();

        if (!m_healthBar)
        {
            Debug.LogError("No Slider component found!");
        }
    }

    private void Start()
    {
        m_playerHealth.Subscribe(this);
    }

    // Methods of the IHealthSubscriber interface
    public void NotifyJustSubscribed(Health healthScript)
    {
        m_healthBar.maxValue = healthScript.MaxHealth;
        m_healthBar.value = healthScript.MaxHealth;
    }

    public void NotifyDamageApplied(Health healthScript, int damage) { }

    public void NotifyHealthChange(Health healthScript, int health)
    {
        m_healthBar.value = health;
    }

    public void NotifyHealthDepleted(Health healthScript) { }
}
