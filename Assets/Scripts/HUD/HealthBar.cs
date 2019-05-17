using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour, IHealthSubscriber
{
    [Header("Health")]
    [SerializeField]
    private Health _health;

    private Slider _healthBar;

    private void Awake()
    {
        _healthBar = GetComponentInChildren<Slider>();

        if (!_healthBar)
        {
            Debug.LogError("No Slider component found!");
        }
    }

    private void Start()
    {
        _health.Subscribe(this);
    }

    // Methods of the IHealthSubscriber interface
    public void NotifyJustSubscribed(Health healthScript)
    {
        _healthBar.maxValue = healthScript.MaxHealth;
        _healthBar.value = healthScript.MaxHealth;
    }

    public void NotifyDamageApplied(Health healthScript, int damage) { }

    public void NotifyHealthChange(Health healthScript, int health)
    {
        _healthBar.value = health;
    }

    public void NotifyHealthDepleted(Health healthScript) { }
}
