using UnityEngine;

public class BossInfo : Shakable, IHealthSubscriber
{
    [Header("Health")]
    [SerializeField]
    private Health m_health;

    private float m_healthPoint;

    private void Start()
    {
        m_health.Subscribe(this);
    }

    // Methods of the IHealthSubscriber interface
    public void NotifyJustSubscribed(Health healthScript)
    {
        m_healthPoint = healthScript.HealthPoint;
    }

    public void NotifyDamageApplied(Health healthScript, int damage) { }

    public void NotifyHealthChange(Health healthScript, int health)
    {
        if (m_healthPoint > health)
        {
            Shake();
        }

        m_healthPoint = healthScript.HealthPoint;
    }

    public void NotifyHealthDepleted(Health healthScript) { }
}
