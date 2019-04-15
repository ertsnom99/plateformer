/*
 * Based on: https://unity3d.com/fr/learn/tutorials/topics/2d-game-creation/player-controller-script 
 **/

using System.Collections.Generic;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class PhysicsObject : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    protected float m_shellRadius = 0.05f;
    
    [SerializeField]
    protected float m_gravityModifier = 2.5f;
    protected float m_currentGravityModifier;

    public float CurrentGravityModifier
    {
        get { return m_currentGravityModifier; }
        private set { m_currentGravityModifier = value; }
    }

    [SerializeField]
    private float m_minGroundNormalY = .4f;
    protected Vector2 m_groundNormal;
    //protected float m_groundAngle;

    public bool IsGrounded { get; private set; }

    [SerializeField]
    protected bool m_debugVelocity = false;

    protected Vector2 m_velocity;

    public Vector2 Velocity
    {
        get { return m_velocity; }
        protected set { m_velocity = value; }
    }

    protected float m_targetHorizontalVelocity;
    protected ContactFilter2D m_contactFilter;
    protected RaycastHit2D[] m_hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> m_hitBufferList = new List<RaycastHit2D>(16);

    protected Dictionary<Collider2D, Rigidbody2D> m_previouslyCollidingGameObject = new Dictionary<Collider2D, Rigidbody2D>();
    protected Dictionary<Collider2D, Rigidbody2D> m_collidingGameObjects = new Dictionary<Collider2D, Rigidbody2D>();

    protected Collider2D m_collider;
    protected Rigidbody2D m_rigidbody2D;

    protected const float MinMoveDistance = 0.001f;
    protected const string OnCollisionEnterMethodName = "OnPhysicsObjectCollisionEnter";
    protected const string OnCollisionExitMethodName = "OnPhysicsObjectCollisionExit";

    protected virtual void Awake()
    {
        m_contactFilter.useTriggers = false;
        // Tells to use the layer settings from the Physics2D settings (the matrix)
        m_contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        m_contactFilter.useLayerMask = true;

        CurrentGravityModifier = m_gravityModifier;

        IsGrounded = false;

        m_collider = GetComponent<Collider2D>();
        m_rigidbody2D = GetComponent<Rigidbody2D>();

        InitialiseRigidbody2D();
    }

    private void InitialiseRigidbody2D()
    {
        m_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        m_rigidbody2D.simulated = true;
        m_rigidbody2D.useFullKinematicContacts = true;
        m_rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        m_rigidbody2D.sleepMode = RigidbodySleepMode2D.NeverSleep;
        m_rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        m_rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    protected virtual void FixedUpdate()
    {
        IsGrounded = false;
        //m_groundAngle = .0f;

        // Update velocity
        Velocity += CurrentGravityModifier * Physics2D.gravity * Time.fixedDeltaTime;
        Velocity = new Vector2 (m_targetHorizontalVelocity, Velocity.y);

        // Create a Vector prependicular to the normal
        Vector2 movementAlongGround = new Vector2(m_groundNormal.y, -m_groundNormal.x);

        // Backup the list of gameObjects that used to collide and clear the original
        m_previouslyCollidingGameObject = new Dictionary<Collider2D, Rigidbody2D>(m_collidingGameObjects);
        m_collidingGameObjects.Clear();

        // The X movement is executed first, then the Y movement is executed. This allows a better control of each type of movement and helps to avoid
        // corner cases. This technic was used in the 16 bit era.
        Vector2 deltaPosition = Velocity * Time.fixedDeltaTime;

        Vector2 movement = movementAlongGround * deltaPosition.x;
        Move(movement, false);
        
        movement = Vector2.up * deltaPosition.y;
        Move(movement, true);

        // Check and broadcast collision exit message
        CheckCollisionExit();

        if (m_debugVelocity)
        {
            Debug.DrawLine(transform.position, transform.position + new Vector3(.0f, Velocity.y, .0f), Color.red);
            Debug.DrawLine(transform.position, transform.position + new Vector3(Velocity.x, .0f, .0f), Color.blue);
            Debug.DrawLine(transform.position, transform.position + new Vector3(m_groundNormal.x, m_groundNormal.y, .0f), Color.yellow);
            Debug.DrawLine(transform.position, transform.position + new Vector3(movementAlongGround.x, movementAlongGround.y, .0f), Color.green);
            Debug.Log(Velocity);
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
            int count = m_rigidbody2D.Cast(movement, m_contactFilter, m_hitBuffer, distance + m_shellRadius);
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

                // Check and broadcast collision enter message
                CheckCollisionEnter(hit);

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
                        //m_groundAngle = m_groundNormal.x != .0f ? Vector2.Angle(m_groundNormal, new Vector2 (m_groundNormal.x, .0f)) : 90.0f;
                    }
                }
                
                Vector2 velocityUsed = yMovement ? Vector2.up * Velocity.y : Vector2.right * Velocity.x;
                // Check how much of the velocity is responsable for going to go throw other colliders
                // Dot calculates how much two vectors are parallel
                // For example Dot([-5, 0], [1, 0]) = -5 , Dot([-5, 0], [-1, 0]) = 5 and Dot([-5, 0], [0, 1]) = 0
                float projection = Vector2.Dot(velocityUsed, currentNormal);

                if (projection < .0f)
                {
                    // Remove part of the velocity
                    if (yMovement)
                    {
                        Velocity = new Vector2(Velocity.x, Velocity.y - (projection * currentNormal).y);
                    }
                    /*else
                    {
                        Velocity = new Vector2(Velocity.x - (projection * currentNormal).x, Velocity.y);
                    }*/
                }

                // Calculate how much movement can be done, before hitting something, considering the ShellRadius  
                float modifiedDistance = hit.distance - m_shellRadius;

                // If the object should move less then what was tought at first, then move less
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

    protected void CheckCollisionEnter(RaycastHit2D hit)
    {
        // If the hitted gameObject wasn't previously hitted
        if (!m_previouslyCollidingGameObject.ContainsKey(hit.collider))
        {
            Vector2 relativeVelocity = hit.rigidbody ? Velocity - hit.rigidbody.velocity : Velocity;

            PhysicsCollision2D physicsObjectCollision2D = new PhysicsCollision2D(m_collider,
                                                                                 hit.collider,
                                                                                 m_rigidbody2D,
                                                                                 hit.rigidbody,
                                                                                 transform,
                                                                                 gameObject,
                                                                                 relativeVelocity,
                                                                                 true,
                                                                                 hit.point);

            hit.collider.SendMessage(OnCollisionEnterMethodName, physicsObjectCollision2D, SendMessageOptions.DontRequireReceiver);
        }

        // Update the list of currently colliding gameObject
        if (!m_collidingGameObjects.ContainsKey(hit.collider))
        {
            m_collidingGameObjects.Add(hit.collider, hit.rigidbody);
        }
    }

    protected void CheckCollisionExit()
    {
        // If a gameObject isn't hitted anymore
        foreach (KeyValuePair<Collider2D, Rigidbody2D> entry in m_previouslyCollidingGameObject)
        {
            if (!m_collidingGameObjects.ContainsKey(entry.Key))
            {
                PhysicsCollision2D physicsObjectCollision2D = new PhysicsCollision2D(m_collider,
                                                                                     entry.Key,
                                                                                     m_rigidbody2D,
                                                                                     entry.Value,
                                                                                     transform,
                                                                                     gameObject,
                                                                                     Vector2.zero,
                                                                                     true);

                entry.Key.SendMessage(OnCollisionExitMethodName, physicsObjectCollision2D, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    protected virtual void OnColliderHitCheck(RaycastHit2D hit) { }

    public void AddVerticalVelocity(float addedVelocity)
    {
        Velocity = new Vector2(Velocity.x, Velocity.y + addedVelocity);
    }
}
