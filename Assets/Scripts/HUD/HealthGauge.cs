using UnityEngine;

public class HealthGauge : MonoBehaviour, IHealthSubscriber
{
    [Header("Health")]
    [SerializeField]
    private Health m_health;

    [Header("Gauge")]
    [SerializeField]
    private Transform m_gaugeFill;
    [SerializeField]
    private float m_gaugeFullRotation = .0f;
    [SerializeField]
    private float m_gaugeEmptyRotation = 180.0f;

    private void Start()
    {
        m_health.Subscribe(this);
    }

    private void UpdateGauge()
    {
        float ZRotation = Mathf.Lerp(m_gaugeEmptyRotation, m_gaugeFullRotation, (float)m_health.HealthPoint / m_health.MaxHealth);
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        m_gaugeFill.rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.z, ZRotation);
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
