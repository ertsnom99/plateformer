using System.Collections;
using UnityEngine;

public interface IUnityBouncingPhysicsObjectSubscriber
{
    void NotifyBounceStarted();
    void NotifyBounceFinished();
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]

public class UnityBouncingPhysicsObject : MonoSubscribable<IUnityBouncingPhysicsObjectSubscriber>
{
    enum BounceStopCondition{ BounceDurationElapsed, MaxBounceCountReached, UnderMinVelocity };

    [Header("Bounce")]
    [SerializeField]
    private float m_bounceFreezeDuration = .1f;
    [SerializeField]
    private bool m_bounceHasStopCondition = true;
    [SerializeField]
    private bool m_freezeWhenStopConditionReached = false;

    [Header("Sound")]
    [SerializeField]
    private AudioClip m_bounceSound;
    [SerializeField]
    private bool m_playBounceSoundOnLastBounce = false;

    [Header("Condition")]
    [SerializeField]
    private BounceStopCondition m_bounceStopCondition;
    [SerializeField]
    private float m_bounceDuration = 3.0f;
    private float m_bounceElapsedTime = .0f;
    [SerializeField]
    private int m_maxBounceCount = 10;
    [SerializeField]
    private float m_minVelocity = 5.0f;

    private bool m_collisionHappened = false;
    private Vector2 m_velocityAtFreeze;
    private int m_bounceCount = 0;

    private bool m_movementFrozen = false;

    public bool MovementFrozen
    {
        get { return m_movementFrozen; }
        private set { m_movementFrozen = value; }
    }

    private Rigidbody2D m_rigidbody2D;
    private AudioSource m_audioSource;

    private void Awake()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        InitialiseRigidbody2D();

        m_audioSource = GetComponent<AudioSource>();

        FreezeMovement(true);
    }

    private void InitialiseRigidbody2D()
    {
        m_rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        m_rigidbody2D.simulated = true;
        m_rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        m_rigidbody2D.sleepMode = RigidbodySleepMode2D.NeverSleep;
        m_rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        m_rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void Launch(Vector2 force)
    {
        m_bounceElapsedTime = .0f;
        m_bounceCount = 0;

        FreezeMovement(false);
        m_rigidbody2D.AddForce(force, ForceMode2D.Impulse);

        // Tell subscribers that the bounce started
        foreach (IUnityBouncingPhysicsObjectSubscriber subscriber in m_subscribers)
        {
            subscriber.NotifyBounceStarted();
        }
    }

    private void Update()
    {
        if (!MovementFrozen)
        {
            m_bounceElapsedTime += Time.deltaTime;

            if (m_bounceHasStopCondition)
            {
                if (m_bounceStopCondition == BounceStopCondition.BounceDurationElapsed && m_bounceElapsedTime >= m_bounceDuration)
                {
                    if (m_freezeWhenStopConditionReached)
                    {
                        FreezeMovement(true);
                    }

                    // Tell subscribers that the bounce finished
                    foreach (IUnityBouncingPhysicsObjectSubscriber subscriber in m_subscribers)
                    {
                        subscriber.NotifyBounceFinished();
                    }
                }
                else if (m_bounceStopCondition == BounceStopCondition.UnderMinVelocity && m_rigidbody2D.velocity.magnitude < m_minVelocity)
                {
                    if (m_freezeWhenStopConditionReached)
                    {
                        FreezeMovement(true);
                    }

                    // Tell subscribers that the bounce finished
                    foreach (IUnityBouncingPhysicsObjectSubscriber subscriber in m_subscribers)
                    {
                        subscriber.NotifyBounceFinished();
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!MovementFrozen)
        {
            m_collisionHappened = false;

            // Rotate to be oriented toward Velocity direction
            float angle = Mathf.Atan2(m_rigidbody2D.velocity.y, m_rigidbody2D.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!MovementFrozen && !m_collisionHappened)
        {
            m_collisionHappened = true;
            m_bounceCount++;

            m_audioSource.PlayOneShot(m_bounceSound);

            if (m_bounceHasStopCondition && m_bounceStopCondition == BounceStopCondition.MaxBounceCountReached && m_bounceCount >= m_maxBounceCount)
            {
                if (m_freezeWhenStopConditionReached)
                {
                    FreezeMovement(true);
                }

                if (!m_playBounceSoundOnLastBounce)
                {
                    m_audioSource.Stop();
                }

                // Tell subscribers that the bounce finished
                foreach (IUnityBouncingPhysicsObjectSubscriber subscriber in m_subscribers)
                {
                    subscriber.NotifyBounceFinished();
                }
            }
            else if (!MovementFrozen)
            {
                StartCoroutine(FreezeForDuration());
            }
        }
    }

    public void FreezeMovement(bool freeze)
    {
        if (freeze)
        {
            m_rigidbody2D.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
            m_velocityAtFreeze = m_rigidbody2D.velocity;
        }
        else
        {
            m_rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            m_rigidbody2D.velocity = m_velocityAtFreeze;
        }

        MovementFrozen = freeze;
    }

    private IEnumerator FreezeForDuration()
    {
        FreezeMovement(true);
        OnFreezeStart();

        yield return new WaitForSeconds(m_bounceFreezeDuration);

        FreezeMovement(false);
        OnFreezeEnd();
    }

    protected virtual void OnFreezeStart() { }

    protected virtual void OnFreezeEnd() { }

    private void OnDisable()
    {
        FreezeMovement(true);
    }
}
