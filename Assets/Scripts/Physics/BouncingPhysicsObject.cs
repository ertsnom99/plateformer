using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBouncingPhysicsObjectSubscriber
{
    void NotifyBounceStarted();
    void NotifyBounceFinished();
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AudioSource))]

public class BouncingPhysicsObject : SubscribablePhysicsObject<IBouncingPhysicsObjectSubscriber>
{
    enum BounceStopCondition { BounceDurationElapsed, MaxBounceCountReached, UnderMinVelocity };

    [Header("Bounce")]
    [SerializeField]
    private float m_bounciness = 1.0f;
    [SerializeField]
    private float m_bounceFreezeDuration = .1f;
    [SerializeField]
    private int m_bounceMaxTry = 100;
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

    private bool m_movementFrozen = true;

    public bool MovementFrozen
    {
        get { return m_movementFrozen; }
        private set { m_movementFrozen = value; }
    }

    private int m_bounceCount = 0;

    private AudioSource m_audioSource;

    protected override void Awake()
    {
        base.Awake();

        m_audioSource = GetComponent<AudioSource>();

        FreezeMovement(true);
    }

    public void Launch(Vector2 force)
    {
        m_bounceElapsedTime = .0f;
        m_bounceCount = 0;

        FreezeMovement(false);
        Velocity = force;

        // Tell subscribers that the bounce started
        foreach (IBouncingPhysicsObjectSubscriber subscriber in m_subscribers)
        {
            subscriber.NotifyBounceStarted();
        }
    }

    protected override void Update()
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
                    foreach (IBouncingPhysicsObjectSubscriber subscriber in m_subscribers)
                    {
                        subscriber.NotifyBounceFinished();
                    }
                }
                else if (m_bounceStopCondition == BounceStopCondition.UnderMinVelocity && Velocity.magnitude < m_minVelocity)
                {
                    if (m_freezeWhenStopConditionReached)
                    {
                        FreezeMovement(true);
                    }

                    // Tell subscribers that the bounce finished
                    foreach (IBouncingPhysicsObjectSubscriber subscriber in m_subscribers)
                    {
                        subscriber.NotifyBounceFinished();
                    }
                }
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (!MovementFrozen)
        {
            // Apply gravity
            Velocity += CurrentGravityModifier * Physics2D.gravity * Time.fixedDeltaTime;

            // Backup the list of gameObjects that used to collide and clear the original
            m_previouslyCollidingGameObject = new Dictionary<Collider2D, Rigidbody2D>(m_collidingGameObjects);
            m_collidingGameObjects.Clear();

            // Move the object
            Vector2 movement = Velocity * Time.fixedDeltaTime;
            Move(movement);

            // Check and broadcast collision exit message
            CheckCollisionExit();

            if (m_debugVelocity)
            {
                Debug.DrawLine(transform.position, transform.position + (Vector3)Velocity, Color.blue);
                Debug.DrawLine(transform.position, transform.position + (Vector3)movement, Color.red);
                Debug.DrawLine(transform.position, transform.position + Vector3.up * m_shellRadius, Color.yellow);
                Debug.Log(Velocity);
            }
        }
    }

    private void Move(Vector2 movement)
    {
        int tryCount = 0;
        float distanceToTravel = movement.magnitude;
        float distanceBeforeHit = movement.magnitude;
        Vector2 currentVelocityDirection = Velocity.normalized;

        // Keeps trying to move until the object travelled the necessary distance
        while (!MovementFrozen && tryCount <= m_bounceMaxTry && distanceToTravel > .0f)
        {
            // Consider that the objet can travel all the remining distance without hitting anything
            distanceBeforeHit = distanceToTravel;

            // Stock the direction of velocity since it is changed when there is a collision
            currentVelocityDirection = Velocity.normalized;

            // Check for collision only if the object moves enough
            if (distanceToTravel > MinMoveDistance)
            {
                // Cast and only consider the first collision
                int count = m_rigidbody2D.Cast(Velocity.normalized, m_contactFilter, m_hitBuffer, distanceToTravel + m_shellRadius);

                if (count > 0 && m_hitBuffer[0])
                {
                    // Update the velocity and the number of bounce
                    OnHit(m_hitBuffer[0].normal);

                    if (!MovementFrozen)
                    {
                        // Stop all movement for a moment
                        StartCoroutine(FreezeMovementOnBounce());
                    }

                    // Check and broadcast collision enter message
                    CheckCollisionEnter(m_hitBuffer[0]);

                    // Update how much distance can be done before hitting something  
                    distanceBeforeHit = m_hitBuffer[0].distance - m_shellRadius;

                    // Will allow inheriting classes to add logic during the hit checks
                    OnColliderHitCheck(m_hitBuffer[0]);
                }
            }

            distanceToTravel -= distanceBeforeHit;

            // Apply the movement
            m_rigidbody2D.position = m_rigidbody2D.position + currentVelocityDirection * distanceBeforeHit;

            tryCount++;
        }

        // Rotate to be oriented toward Velocity direction
        float angle = Mathf.Atan2(Velocity.y, Velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnHit(Vector2 normal)
    {
        // Reflect the direction of the velocity along the normal
        Velocity = Velocity.magnitude * m_bounciness * Vector2.Reflect(Velocity.normalized, normal).normalized;

        // Increment and check the number of bounce
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
            foreach (IBouncingPhysicsObjectSubscriber subscriber in m_subscribers)
            {
                subscriber.NotifyBounceFinished();
            }
        }
    }

    public void FreezeMovement(bool freeze)
    {
        MovementFrozen = freeze;
    }

    private IEnumerator FreezeMovementOnBounce()
    {
        FreezeMovement(true);
        OnFreezeStart();

        yield return new WaitForSeconds(m_bounceFreezeDuration);

        FreezeMovement(false);
        OnFreezeEnd();
    }

    protected virtual void OnFreezeStart() { }

    protected virtual void OnFreezeEnd() { }

    private void OnPhysicsObjectCollisionEnter(PhysicsCollision2D physicsObjectCollision2D)
    {
        if (!MovementFrozen)
        {
            OnHit(physicsObjectCollision2D.Normal);
        }
    }

    private void OnDisable()
    {
        FreezeMovement(true);
    }
}
