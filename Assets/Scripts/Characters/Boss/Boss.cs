using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Health))]

public class Boss : MonoBehaviour, IBreakableInterrupterSubscriber
{
    [Header("Damage")]
    [SerializeField]
    private BreakableInterrupter[] m_interrupters;
    [SerializeField]
    private int m_interrupterBreakableDamage = 10;

    private Health m_health;
    
    private void Awake()
    {
        m_health = GetComponent<Health>();
    }

    private void Start()
    {
        foreach (BreakableInterrupter interrupter in m_interrupters)
        {
            interrupter.Subscribe(this);
        }
    }

    // Methods of the IInterrupterBreakable interface
    public void NotifyInterrupterBreaked(BreakableInterrupter breakableInterrupter)
    {
        m_health.Damage(m_interrupterBreakableDamage);
    }
}
