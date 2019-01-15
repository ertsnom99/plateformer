using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShakableHealthBar : Shakable, IHealthSubscriber
{
    [Header("Health")]
    [SerializeField]
    private Health m_health;

    private Slider m_healthBar;
    private float m_healthPoint;

    protected override void Awake()
    {
        base.Awake();

        m_healthBar = GetComponentInChildren<Slider>();

        if (!m_healthBar)
        {
            Debug.LogError("No Slider component found!");
        }
    }

    private void Start()
    {
        m_health.Subscribe(this);
    }
    
    // Methods of the IHealthSubscriber interface
    public void NotifyJustSubscribed(Health healthScript)
    {
        m_healthBar.maxValue = healthScript.MaxHealth;
        m_healthBar.value = healthScript.MaxHealth;
        
        m_healthPoint = healthScript.HealthPoint;
    }

    public void NotifyDamageApplied(Health healthScript, int damage) { }

    public void NotifyHealthChange(Health healthScript, int health)
    {
        m_healthBar.value = health;
        
        if (m_healthPoint > health)
        {
            Shake();
        }

        m_healthPoint = healthScript.HealthPoint;
    }

    public void NotifyHealthDepleted(Health healthScript) { }
}