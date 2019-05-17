using UnityEngine;

public class HealthGauge : MonoBehaviour, IHealthSubscriber
{
    [Header("Health")]
    [SerializeField]
    private Health _health;

    [Header("Gauge")]
    [SerializeField]
    private Transform _gaugeFill;
    [SerializeField]
    private float _gaugeFullRotation = .0f;
    [SerializeField]
    private float _gaugeEmptyRotation = 180.0f;

    private void Start()
    {
        _health.Subscribe(this);
    }

    private void UpdateGauge()
    {
        float ZRotation = Mathf.Lerp(_gaugeEmptyRotation, _gaugeFullRotation, (float)_health.HealthPoint / _health.MaxHealth);
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        _gaugeFill.rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.z, ZRotation);
    }

    // Methods of the IHealthSubscriber interface
    public void NotifyJustSubscribed(Health healthScript)
    {
        UpdateGauge();
    }

    public void NotifyDamageApplied(Health healthScript, int damage) { }

    public void NotifyHealthChange(Health healthScript, int health)
    {
        UpdateGauge();
    }

    public void NotifyHealthDepleted(Health healthScript) { }
}
