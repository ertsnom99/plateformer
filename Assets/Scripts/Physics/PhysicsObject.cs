/*
 * Based on: https://unity3d.com/fr/learn/tutorials/topics/2d-game-creation/player-controller-script 
 **/

using System.Collections.Generic;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Rigidbody2D))]

public class PhysicsObject : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    protected float m_gravityModifier = 1.4f;
    protected float m_currentGravityModifier;

    [SerializeField]
    private float m_minGroundNormalY = .4f;
    protected Vector2 m_groundNormal;
    protected float m_groundAngle;

    public bool IsGrounded { get; private set; }

    [SerializeField]
    private bool m_debugVelocity = false;

    protected Vector2 m_velocity;
    protected float m_targetHorizontalVelocity;
    protected ContactFilter2D m_contactFilter;
    protected RaycastHit2D[] m_hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> m_hitBufferList = new List<RaycastHit2D>(16);
    
    protected Rigidbody2D m_rigidbody2D;

    protected const float MinMoveDistance = 0.001f;
    protected const float ShellRadius = 0.015f;

    protected virtual void Awake()
    {
        m_contactFilter.useTriggers = false;
        // Tells to use the layer settings from the Physics2D settings (the matrix)
        m_contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        m_contactFilter.useLayerMask = true;

        m_currentGravityModifier = m_gravityModifier;

        IsGrounded = false;

        m_rigidbody2D = GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        IsGrounded = false;
        m_groundAngle = .0f;

        // Update velocity
        m_velocity += m_currentGravityModifier * Physics2D.gravity * Time.fixedDeltaTime;
        m_velocity.x = m_targetHorizontalVelocity;

        // Create a Vector prependicular to the normal
        Vector2 movementAlongGround = new Vector2(m_groundNormal.y, -m_groundNormal.x);

        // The X movement is executed first, then the Y movement is executed. This allows a better control of each type of movement and helps to avoid
        // corner cases. This technic was used in the 16 bit era.
        Vector2 deltaPosition = m_velocity * Time.fixedDeltaTime;

        Vector2 movement = movementAlongGround * deltaPosition.x;
        Move(movement, false);
        
        movement = Vector2.up * deltaPosition.y;
        Move(movement, true);

        if (m_debugVelocity)
        {
            Debug.DrawLine(transform.position, transform.position + new Vector3(.0f, m_velocity.y, .0f), Color.red);
            Debug.DrawLine(transform.position, transform.position + new Vector3(m_velocity.x, .0f, .0f), Color.blue);
            Debug.DrawLine(transform.position, transform.position + new Vector3(m_groundNormal.x, m_groundNormal.y, .0f), Color.yellow);
            Debug.DrawLine(transform.position, transform.position + new Vector3(movementAlongGround.x, movementAlongGround.y, .0f), Color.green);
            Debug.Log(m_velocity);
        }
    }

    protected virtual void Update()
    {
        m_targetHorizontalVelocity = .0f;
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
            // m_hitBuffer has always the same number of elements in it, but some might be null.
            // The purpose of the transfer is to only keep the actual result of the cast
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
                    IsGrounded = true;

                    if (yMovement)
                    {
                        // Update ground normal
                        m_groundNormal = currentNormal;
                        currentNormal.x = 0;

                        // update ground angle
                        m_groundAngle = m_groundNormal.x != .0f ? Vector2.Angle(m_groundNormal, new Vector2 (m_groundNormal.x, .0f)) : 90.0f;
                    }
                }


                Vector2 velocityUsed = yMovement ? Vector2.up * m_velocity.y : Vector2.right * m_velocity.x;
                // Check how much of the velocity is responsable for going to go throw other colliders
                // Dot calculates how much two vectors are parallel
                // For example Dot([-5, 0], [1, 0]) = -5 , Dot([-5, 0], [-1, 0]) = 5 and Dot([-5, 0], [0, 1]) = 0
                float projection = Vector2.Dot(velocityUsed, currentNormal);

                if (projection < .0f)
                {
                    // Remove part of the velocity
                    if (yMovement)
                    {
                        m_velocity.y -= (projection * currentNormal).y;
                    }
                    else
                    {
                        m_velocity.x -= (projection * currentNormal).x;
                    }
                }

                // Calculate how much movement can be done, before hitting something, considering the ShellRadius  
                float modifiedDistance = hit.distance - ShellRadius;
                // If, after calculation, the object should move less then what was tought at first, then move less
                distance = modifiedDistance < distance ? modifiedDistance : distance;

                // Will allow inheriting classes to add logic during the hit checks
                OnColliderHitCheck(hit);
            }
        }

        // Apply the movement
        m_rigidbody2D.position = m_rigidbody2D.position + movement.normalized * distance;
        // Updating m_rigidbody2D.position doesn't update transform.position
        // It is only updated before or at the start of next FixedUpdate() (note sure, should be double checked)
        //transform.position = m_rigidbody2D.position;
    }

    protected virtual void OnColliderHitCheck(RaycastHit2D hit) { }

    public void AddVerticalVelocity(float addedVelocity)
    {
        m_velocity.y += addedVelocity;
    }
}
