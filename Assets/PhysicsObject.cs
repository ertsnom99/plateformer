using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    [SerializeField]
    private float m_gravityModifier = 1.0f;
    [SerializeField]
    private float m_minGroundNormalY = .65f;

    protected bool m_isGrounded = false;
    protected Vector2 m_groundNormal;

    protected Vector2 m_velocity;
    protected Vector2 m_targetVelocity;
    protected ContactFilter2D m_contactFilter;
    protected RaycastHit2D[] m_hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> m_hitBufferList = new List<RaycastHit2D>(16);
    
    protected Rigidbody2D m_rigidbody2D;

    protected const float MinMoveDistance = 0.001f;
    protected const float ShellRadius = 0.01f;

    protected virtual void Awake()
    {
        m_contactFilter.useTriggers = false;
        // Tells to use the layer settings from the Physics2D settings (the matrix)
        m_contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        m_contactFilter.useLayerMask = true;

        m_rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        m_isGrounded = false;

        // Update velocity
        m_velocity += m_gravityModifier * Physics2D.gravity * Time.fixedDeltaTime;
        m_velocity.x = m_targetVelocity.x;

        // Create a Vector prependicular to the normal
        Vector2 moveAlongGround = new Vector2(m_groundNormal.y, -m_groundNormal.x);

        // Quantity of movement to be done during this fixed update, based on the velocity
        Vector2 deltaPosition = m_velocity * Time.fixedDeltaTime;

        // The X movement is executed first, then the Y movement is executed. This allows a better control of each type of movement and helps to avoid
        // corner cases. This tehcnic was used in the 16 bit era.
        Vector2 movement = moveAlongGround * deltaPosition.x;
        Move(movement, false);

        movement = Vector2.up * deltaPosition.y;
        Move(movement, true);
    }

    private void Update()
    {
        m_targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity() { }

    private void Move(Vector2 movement, bool yMovement)
    {
        float distance = movement.magnitude;

        // Check for collision only if the object moves enough
        if (distance > MinMoveDistance)
        {
            int count = m_rigidbody2D.Cast(movement, m_contactFilter, m_hitBuffer, distance + ShellRadius);
            m_hitBufferList.Clear();

            // Transfer hits to m_hitBufferList
            for (int i = 0; i < count; i++)
            {
                m_hitBufferList.Add(m_hitBuffer[i]);
            }

            foreach (RaycastHit2D hit in m_hitBufferList)
            {
                Vector2 currentNormal = hit.normal;

                // Check if the object is grounded
                if (currentNormal.y > m_minGroundNormalY)
                {
                    m_isGrounded = true;

                    if (yMovement)
                    {
                        m_groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                // Check how much the object is going to go throw other colliders
                float projection = Vector2.Dot(m_velocity, currentNormal);

                if (projection < .0f)
                {
                    // Remove part of the velocity to prevent from going throw colliders
                    m_velocity -= projection * currentNormal;
                }

                // Calculate how much movement can be done, before touching the ground, considering the ShellRadius  
                float modifiedDistance = hit.distance - ShellRadius;
                // If, after calculation, we should move less then what we tought at first, then move less
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }

        // Apply the movement
        m_rigidbody2D.position = m_rigidbody2D.position + movement.normalized * distance;
    }
}
