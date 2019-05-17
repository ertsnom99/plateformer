using UnityEngine;

public interface IDestroyableInterrupterSubscriber
{
    void NotifyInterrupterDestroyed(DestroyableInterrupter destroyableInterrupter);
}

public enum WayToBreak
{
    Ram = 0,
    DirectDamage,
    Any
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(DestroyableInterrupterHealth))]

public class DestroyableInterrupter : MonoSubscribable<IDestroyableInterrupterSubscriber>, IPhysicsObjectCollisionListener, IHealthSubscriber
{
    [Header("Break method")]
    [SerializeField]
    private WayToBreak _wayToDamage;

    [Header("Ram")]
    [SerializeField]
    private bool _onlyPlayerCanRam = false;
    [SerializeField]
    private Vector2[] _localVectorsForAngleCalculation;
    [SerializeField]
    private float _maxAngleToDamage = 45.0f;
    [SerializeField]
    private float _minVelocityToDamage = 30.0f;
    [SerializeField]
    private int _damageDealtOnHit = 1;

    public bool IsBroken { get; private set; }

    private Animator _animator;
    private DestroyableInterrupterHealth _health;

    protected int IsDestroyedParamHashId = Animator.StringToHash(IsDestroyedParamNameString);

    public const string IsDestroyedParamNameString = "IsDestroyed";

    private void Awake()
    {
        IsBroken = false;
        
        _animator = GetComponent<Animator>();
        _health = GetComponent<DestroyableInterrupterHealth>();

        _health.SetCanBeDirectlyDamage(_wayToDamage == WayToBreak.DirectDamage || _wayToDamage == WayToBreak.Any);
        _health.Subscribe(this);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        OnImpact(col.relativeVelocity, col.collider.gameObject);
    }
    
    private void OnImpact(Vector2 relativeVelocity, GameObject collidingGameObject)
    {
        if (!IsBroken && (!_onlyPlayerCanRam || collidingGameObject.CompareTag(GameManager.PlayerTag)))
        {
            switch (_wayToDamage)
            {
                case WayToBreak.Ram:

                    foreach(Vector2 localVectorForAngleCalculation in _localVectorsForAngleCalculation)
                    {
                        float angle = Vector2.Angle(transform.TransformDirection(localVectorForAngleCalculation), relativeVelocity);

                        if (angle <= _maxAngleToDamage && relativeVelocity.magnitude >= _minVelocityToDamage)
                        {
                            _health.ForceDamage(_damageDealtOnHit);
                            break;
                        }
                    }

                    break;
            }
        }
    }

    private void Break()
    {
        foreach (IDestroyableInterrupterSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyInterrupterDestroyed(this);
        }

        IsBroken = true;
        _animator.SetBool(IsDestroyedParamHashId, true);
    }

    // Methods of the IPhysicsObjectCollisionListener interface
    public void OnPhysicsObjectCollisionExit(PhysicsCollision2D collision) { }

    public void OnPhysicsObjectCollisionEnter(PhysicsCollision2D collision)
    {
        OnImpact(collision.RelativeVelocity, collision.Collider.gameObject);
    }

    // Methods of the IHealthSubscriber interface
    public void NotifyJustSubscribed(Health healthScript) { }

    public void NotifyDamageApplied(Health healthScript, int damage) { }

    public void NotifyHealthChange(Health healthScript, int health) { }

    public void NotifyHealthDepleted(Health healthScript)
    {
        if (!IsBroken)
        {
            Break();
        }
    }
}
