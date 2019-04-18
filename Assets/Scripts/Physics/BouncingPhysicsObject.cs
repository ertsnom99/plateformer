using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingPhysicsObject : PhysicsObject
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
    private BounceStopCondition m_bounceStopCondition;
    [SerializeField]
    private float m_bounceDuration = 3.0f;
    private float m_bounceElapsedTime = .0f;
    [SerializeField]
    private int m_maxBounceCount = 10;
    [SerializeField]
    private float m_minVelocity = 5.0f;

    [SerializeField]
    private Vector2 m_force = new Vector2(-10.0f, -2.0f);

    private bool m_movementFrozen = true;
    private int m_bounceCount = 0;

    private void Start()
    {
        m_movementFrozen = true;
    }

    public void Throw(Vector2 force)
    {
        m_bounceElapsedTime = .0f;
        m_bounceCount = 0;

        Velocity = force;
        m_movementFrozen = false;
    }

    protected override void Update()
    {
        if (!m_movementFrozen)
        {
            m_bounceElapsedTime += Time.deltaTime;

            if (m_bounceStopCondition == BounceStopCondition.BounceDurationElapsed && m_bounceElapsedTime >= m_bounceDuration)
            {
                m_movementFrozen = true;
            }
            else if (m_bounceStopCondition == BounceStopCondition.UnderMinVelocity && Velocity.magnitude < m_minVelocity)
            {
                m_movementFrozen = true;
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (!m_movementFrozen)
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
        if (!m_movementFrozen)
        {
            int tryCount = 0;
            float distanceToTravel = movement.magnitude;
            float distanceBeforeHit = movement.magnitude;
            Vector2 currentVelocityDirection = Velocity.normalized;

            // Keeps trying to move until the object travelled the necessary distance
            while (!m_movementFrozen && tryCount <= m_bounceMaxTry && distanceToTravel > .0f)
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
                        // Update the velocity
                        UpdateVelocityOnHit(m_hitBuffer[0].normal);

                        if (!m_movementFrozen)
                        {
                            // Stop all movement for a moment
                            StartCoroutine(FreezeMovement());
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
    }

    private void UpdateVelocityOnHit(Vector2 normal)
    {
        Velocity = Velocity.magnitude * m_bounciness * Vector2.Reflect(Velocity.normalized, normal).normalized;
        m_bounceCount++;

        if (m_bounceStopCondition == BounceStopCondition.MaxBounceCountReached && m_bounceCount >= m_maxBounceCount)
        {
            m_movementFrozen = true;
        }
    }

    private IEnumerator FreezeMovement()
    {
        m_movementFrozen = true;
        OnFreezeStart();

        yield return new WaitForSeconds(m_bounceFreezeDuration);

        m_movementFrozen = false;
        OnFreezeEnd();
    }

    protected virtual void OnFreezeStart() { }

    protected virtual void OnFreezeEnd() { }

    private void OnPhysicsObjectCollisionEnter(PhysicsCollision2D physicsObjectCollision2D)
    {
        if (!m_movementFrozen)
        {
            UpdateVelocityOnHit(physicsObjectCollision2D.Normal);
        }
    }
}
