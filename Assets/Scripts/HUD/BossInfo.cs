using UnityEngine;

public class BossInfo : Shakable, IHealthSubscriber
{
    [Header("Health")]
    [SerializeField]
    private Health _health;

    private float _healthPoint;

    private void Start()
    {
        _health.Subscribe(this);
    }

    // Methods of the IHealthSubscriber interface
    public void NotifyJustSubscribed(Health healthScript)
    {
        _healthPoint = healthScript.HealthPoint;
    }

    public void NotifyDamageCalled(Health healthScript, int damage) { }

    public void NotifyHealCalled(Health healthScript, int gain) { }

    public void NotifyHealthChange(Health healthScript, int health)
    {
        if (_healthPoint > health)
        {
            Shake();
        }

        _healthPoint = healthScript.HealthPoint;
    }

    public void NotifyHealthDepleted(Health healthScript) { }
}
