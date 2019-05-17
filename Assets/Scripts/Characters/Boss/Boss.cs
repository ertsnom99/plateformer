using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Health))]

public class Boss : MonoBehaviour, IDestroyableInterrupterSubscriber
{
    [Header("Damage")]
    [SerializeField]
    private DestroyableInterrupter[] _interrupters;
    [SerializeField]
    private int _interrupterBreakableDamage = 10;

    private Health _health;
    
    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void Start()
    {
        foreach (DestroyableInterrupter interrupter in _interrupters)
        {
            interrupter.Subscribe(this);
        }
    }

    // Methods of the IInterrupterBreakable interface
    public void NotifyInterrupterDestroyed(DestroyableInterrupter destroyableInterrupter)
    {
        _health.Damage(_interrupterBreakableDamage);
    }
}
