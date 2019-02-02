using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Health))]

public class Boss : MonoBehaviour, IDestroyableInterrupterSubscriber
{
    [Header("Damage")]
    [SerializeField]
    private DestroyableInterrupter[] m_interrupters;
    [SerializeField]
    private int m_interrupterBreakableDamage = 10;

    private Health m_health;
    
    private void Awake()
    {
        m_health = GetComponent<Health>();
    }

    private void Start()
    {
        foreach (DestroyableInterrupter interrupter in m_interrupters)
        {
            interrupter.Subscribe(this);
        }
    }

    // Methods of the IInterrupterBreakable interface
    public void NotifyInterrupterDestroyed(DestroyableInterrupter destroyableInterrupter)
    {
        m_health.Damage(m_interrupterBreakableDamage);
    }
}
