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

public class DestroyableInterrupter : MonoSubscribable<IDestroyableInterrupterSubscriber>, IHealthSubscriber
{
    [Header("Break method")]
    [SerializeField]
    private WayToBreak m_wayToDamage;

    [Header("Ram")]
    [SerializeField]
    private bool m_onlyPlayerCanRam = false;
    [SerializeField]
    private Vector2[] m_localVectorsForAngleCalculation;
    [SerializeField]
    private float m_maxAngleToDamage = 45.0f;
    [SerializeField]
    private float m_minVelocityToDamage = 30.0f;
    [SerializeField]
    private int m_damageDealtOnHit = 1;

    public bool IsBroken { get; private set; }

    private Animator m_animator;
    private DestroyableInterrupterHealth m_health;

    protected int m_isDestroyedParamHashId = Animator.StringToHash(IsDestroyedParamNameString);

    public const string IsDestroyedParamNameString = "IsDestroyed";

    private void Awake()
    {
        IsBroken = false;
        
        m_animator = GetComponent<Animator>();
        m_health = GetComponent<DestroyableInterrupterHealth>();

        m_health.SetCanBeDirectlyDamage(m_wayToDamage == WayToBreak.DirectDamage || m_wayToDamage == WayToBreak.Any);
        m_health.Subscribe(this);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        OnImpact(col.relativeVelocity, col.collider.gameObject);
    }

    private void OnPhysicsObjectCollisionEnter(PhysicsCollision2D physicsObjectCollision2D)
    {
        OnImpact(physicsObjectCollision2D.RelativeVelocity, physicsObjectCollision2D.Collider.gameObject);
    }
    
    private void OnImpact(Vector2 relativeVelocity, GameObject collidingGameObject)
    {
        if (!IsBroken && (!m_onlyPlayerCanRam || collidingGameObject.CompareTag(GameManager.PlayerTag)))
        {
            switch (m_wayToDamage)
            {
                case WayToBreak.Ram:

                    foreach(Vector2 localVectorForAngleCalculation in m_localVectorsForAngleCalculation)
                    {
                        float angle = Vector2.Angle(transform.TransformDirection(localVectorForAngleCalculation), relativeVelocity);

                        if (angle <= m_maxAngleToDamage && relativeVelocity.magnitude >= m_minVelocityToDamage)
                        {
                            m_health.ForceDamage(m_damageDealtOnHit);
                            break;
                        }
                    }

                    break;
            }
        }
    }

    private void Break()
    {
        foreach (IDestroyableInterrupterSubscriber subscriber in m_subscribers)
        {
            subscriber.NotifyInterrupterDestroyed(this);
        }

        IsBroken = true;
        m_animator.SetBool(m_isDestroyedParamHashId, true);
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
