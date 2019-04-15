using System.Collections.Generic;
using UnityEngine;

public class BouncingPhysicsObject : PhysicsObject
{
    [Header("Bounce")]
    [SerializeField]
    private float m_bounciness = 1.0f;
    [SerializeField]
    private bool m_applyGravity = true;
    [SerializeField]
    private int m_bounceMaxTry = 100;
    [SerializeField]
    private Vector2 m_force = new Vector2(-10.0f, -2.0f);

    private int m_bounceCount = 0;

    private void Start()
    {
        Velocity = m_force;
    }

    protected override void FixedUpdate()
    {
        // Apply gravity
        if (m_applyGravity)
        {
            Velocity += CurrentGravityModifier * Physics2D.gravity * Time.fixedDeltaTime;
        }

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

    protected override void Update() { }

    private void Move(Vector2 movement)
    {
        int tryCount = 0;
        float distanceToTravel = movement.magnitude;
        float distanceBeforeHit = movement.magnitude;

        // Keeps trying to move until the object travelled the necessary distance
        while (tryCount <= m_bounceMaxTry && distanceToTravel > .0f)
        {
            // Consider that the objet can travel all the remining distance without hitting anything
            distanceBeforeHit = distanceToTravel;

            // Check for collision only if the object moves enough
            if (distanceToTravel > MinMoveDistance)
            {
                // Cast and only consider the first collision
                int count = m_rigidbody2D.Cast(Velocity.normalized, m_contactFilter, m_hitBuffer, distanceToTravel + m_shellRadius);

                if (count > 0 && m_hitBuffer[0])
                {
                    // Update the velocity
                    UpdateVelocityOnHit(m_hitBuffer[0].normal);

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
            m_rigidbody2D.position = m_rigidbody2D.position + Velocity.normalized * distanceBeforeHit;

            tryCount++;
        }
    }

    private void UpdateVelocityOnHit(Vector2 normal)
    {
        Velocity = Velocity.magnitude * m_bounciness * Vector2.Reflect(Velocity.normalized, normal).normalized;
        m_bounceCount++;
    }

    private void OnPhysicsObjectCollisionEnter(PhysicsCollision2D physicsObjectCollision2D)
    {
        UpdateVelocityOnHit(physicsObjectCollision2D.Normal);
    }
}
