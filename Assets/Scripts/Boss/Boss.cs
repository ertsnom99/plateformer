using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Health))]

public class Boss : MonoBehaviour, IDestroyableGameObjectSubscriber
{
    [Header("Damage")]
    [SerializeField]
    private DestroyableGameObject[] _destroyableGameObjects;
    [SerializeField]
    private int _interrupterBreakableDamage = 10;

    private Health _health;
    
    private void Awake()
    {
        _health = GetComponent<Health>();
    }

    private void Start()
    {
        foreach (DestroyableGameObject interrupter in _destroyableGameObjects)
        {
            interrupter.Subscribe(this);
        }
    }

    // Methods of the IDestroyableGameObjectSubscriber interface
    public void NotifyGameObjectDestroyed(DestroyableGameObject destroyableInterrupter)
    {
        _health.Damage(_interrupterBreakableDamage);
    }
}
