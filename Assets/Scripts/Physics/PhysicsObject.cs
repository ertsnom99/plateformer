/*
 * Based on: https://unity3d.com/fr/learn/tutorials/topics/2d-game-creation/player-controller-script 
 **/

using System.Collections.Generic;
using UnityEngine;

public interface IPhysicsObjectCollisionListener
{
    void OnPhysicsObjectCollisionEnter(PhysicsCollision2D collision);
    void OnPhysicsObjectCollisionExit(PhysicsCollision2D collision);
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Rigidbody2D))]

public class PhysicsObject : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField]
    protected Collider2D Collider;
    [SerializeField]
    private bool _updateContactFilterLayerMask = false;

    [Header("Movement")]
    [SerializeField]
    protected float ShellRadius = 0.05f;
    
    [SerializeField]
    protected float GravityModifier = 1.0f;

    public float CurrentGravityModifier { get; protected set; }

    [SerializeField]
    protected float MaxFallVelocity = 44.0f;
    [SerializeField]
    protected float FallModifier = 1.0f;

    [SerializeField]
    private float _minGroundNormalY = .4f;
    protected Vector2 GroundNormal;
    //protected float m_groundAngle;

    public bool IsGrounded { get; private set; }
    
    [SerializeField]
    protected bool DebugVelocity = false;

    public Vector2 Velocity { get; protected set; }

    protected float TargetHorizontalVelocity;
    protected ContactFilter2D ContactFilter;
    protected RaycastHit2D[] HitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> HitBufferList = new List<RaycastHit2D>(16);

    protected Dictionary<Collider2D, IPhysicsObjectCollisionListener[]> PreviouslyCollidingGameObject = new Dictionary<Collider2D, IPhysicsObjectCollisionListener[]>();
    protected Dictionary<Collider2D, IPhysicsObjectCollisionListener[]> CollidingGameObjects = new Dictionary<Collider2D, IPhysicsObjectCollisionListener[]>();

    private IPhysicsObjectCollisionListener[] _collisionListeners;

    protected Rigidbody2D Rigidbody2D;

    protected const float MinMoveDistance = 0.001f;

    protected virtual void Awake()
    {
        // Tells to use the layer settings from the Physics2D settings (the matrix)
        ContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        ContactFilter.useLayerMask = true;
        ContactFilter.useTriggers = false;

        CurrentGravityModifier = GravityModifier;

        IsGrounded = false;

        _collisionListeners = GetComponentsInChildren<IPhysicsObjectCollisionListener>();

        if (!Collider)
        {
            Debug.LogError("No collider was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        Rigidbody2D = GetComponent<Rigidbody2D>();

        InitialiseRigidbody2D();
    }

    private void InitialiseRigidbody2D()
    {
        Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        Rigidbody2D.simulated = true;
        Rigidbody2D.useFullKinematicContacts = true;
        Rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        Rigidbody2D.sleepMode = RigidbodySleepMode2D.NeverSleep;
        Rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    protected virtual void FixedUpdate()
    {
        if (_updateContactFilterLayerMask)
        {
            ContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        }

        IsGrounded = false;
        //m_groundAngle = .0f;

        // Update velocity
        Vector2 yVelocityAdded = CurrentGravityModifier * Physics2D.gravity * Time.fixedDeltaTime;

        if (Velocity.y < .0f)
        {
            yVelocityAdded *= FallModifier;
        }

        Velocity += yVelocityAdded;

        if (Velocity.y < -MaxFallVelocity)
        {
            Velocity = new Vector2(TargetHorizontalVelocity, -MaxFallVelocity);
        }
        else
        {
            Velocity = new Vector2(TargetHorizontalVelocity, Velocity.y);
        }
        
        // Create a Vector prependicular to the normal
        Vector2 movementAlongGround = new Vector2(GroundNormal.y, -GroundNormal.x);

        // Backup the list of gameObjects that used to collide and clear the original
        PreviouslyCollidingGameObject = new Dictionary<Collider2D, IPhysicsObjectCollisionListener[]>(CollidingGameObjects);
        CollidingGameObjects.Clear();

        // The X movement is executed first, then the Y movement is executed. This allows a better control of each type of movement and helps to avoid
        // corner cases. This technic was used in the 16 bit era.
        Vector2 deltaPosition = Velocity * Time.fixedDeltaTime;

        Vector2 movement = movementAlongGround * deltaPosition.x;
        Move(movement, false);
        
        movement = Vector2.up * deltaPosition.y;
        Move(movement, true);

        // Check and call collision exit methods
        CheckCollisionExit();

        if (DebugVelocity)
        {
            Debug.DrawLine(transform.position, transform.position + new Vector3(.0f, Velocity.y, .0f), Color.red);
            Debug.DrawLine(transform.position, transform.position + new Vector3(Velocity.x, .0f, .0f), Color.blue);
            //Debug.DrawLine(transform.position, transform.position + new Vector3(m_groundNormal.x, m_groundNormal.y, .0f), Color.yellow);
            //Debug.DrawLine(transform.position, transform.position + new Vector3(movementAlongGround.x, movementAlongGround.y, .0f), Color.green);
            Debug.Log(Velocity);
        }
    }

    protected virtual void Update()
    {
        TargetHorizontalVelocity = .0f;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity() { }

    private void Move(Vector2 movement, bool yMovement)
    {
        float distance = movement.magnitude;

        // Check for collision only if the object moves enough
        if (distance > MinMoveDistance)
        {
            int count = Collider.Cast(movement, ContactFilter, HitBuffer, distance + ShellRadius);
            HitBufferList.Clear();

            // Transfer hits to m_hitBufferList
            // m_hitBuffer has always the same number of elements in it, but some might be null.
            // The purpose of the transfer is to only keep the actual result of the cast
            for (int i = 0; i < count; i++)
            {
                HitBufferList.Add(HitBuffer[i]);
            }

            foreach (RaycastHit2D hit in HitBufferList)
            {
                Vector2 currentNormal = hit.normal;

                // Check and call collision enter methods
                CheckCollisionEnter(hit);

                // Check if the object is grounded
                if (currentNormal.y > _minGroundNormalY)
                {
                    IsGrounded = true;

                    if (yMovement)
                    {
                        // Update ground normal
                        GroundNormal = currentNormal;
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
                float modifiedDistance = hit.distance - ShellRadius;

                // If the object should move less then what was tought at first, then move less
                distance = modifiedDistance < distance ? modifiedDistance : distance;

                // Will allow inheriting classes to add logic during the hit checks
                OnColliderHitCheck(hit);
            }
        }

        // Apply the movement
        Rigidbody2D.position = Rigidbody2D.position + movement.normalized * distance;

        // Updating m_rigidbody2D.position doesn't update transform.position
        // It is only updated before or at the start of next FixedUpdate() (note sure, should be double checked)
        //transform.position = m_rigidbody2D.position;
    }

    protected void CheckCollisionEnter(RaycastHit2D hit)
    {
        IPhysicsObjectCollisionListener[] collisionListeners;
        
        // If the hitted gameObject wasn't previously hitted
        if (!PreviouslyCollidingGameObject.ContainsKey(hit.collider))
        {
            // Call OnPhysicsObjectCollisionEnter on all script, of this gameobject, that implement the interface
            Vector2 relativeVelocity = hit.rigidbody ? hit.rigidbody.velocity - Velocity : -Velocity;

            PhysicsCollision2D physicsObjectCollision2D = new PhysicsCollision2D(hit.collider,
                                                                                 Collider,
                                                                                 hit.rigidbody,
                                                                                 Rigidbody2D,
                                                                                 hit.transform,
                                                                                 hit.collider.gameObject,
                                                                                 relativeVelocity,
                                                                                 true,
                                                                                 hit.point,
                                                                                 hit.normal);

            foreach (IPhysicsObjectCollisionListener collisionListener in _collisionListeners)
            {
                collisionListener.OnPhysicsObjectCollisionEnter(physicsObjectCollision2D);
            }

            // Call OnPhysicsObjectCollisionEnter on all script, of the hitted gameobject, that implement the interface
            collisionListeners = hit.collider.GetComponents<IPhysicsObjectCollisionListener>();

            if (collisionListeners.Length > 0)
            {
                relativeVelocity = hit.rigidbody ? Velocity - hit.rigidbody.velocity : Velocity;

                physicsObjectCollision2D = new PhysicsCollision2D(Collider,
                                                                  hit.collider,
                                                                  Rigidbody2D,
                                                                  hit.rigidbody,
                                                                  transform,
                                                                  gameObject,
                                                                  relativeVelocity,
                                                                  true,
                                                                  hit.point,
                                                                  -hit.normal);

                foreach (IPhysicsObjectCollisionListener listener in collisionListeners)
                {
                    listener.OnPhysicsObjectCollisionEnter(physicsObjectCollision2D);
                }
            }
        }
        else
        {
            collisionListeners = PreviouslyCollidingGameObject[hit.collider];
        }

        // Update the list of currently colliding gameObject
        if (!CollidingGameObjects.ContainsKey(hit.collider))
        {
            CollidingGameObjects.Add(hit.collider, collisionListeners);
        }
    }

    protected void CheckCollisionExit()
    {
        // If a gameObject isn't hitted anymore
        foreach (KeyValuePair<Collider2D, IPhysicsObjectCollisionListener[]> entry in PreviouslyCollidingGameObject)
        {
            // Call OnPhysicsObjectCollisionExit on all script, of the hitted gameobject, that implement the interface
            if (entry.Key && !CollidingGameObjects.ContainsKey(entry.Key))
            {
                // Call OnPhysicsObjectCollisionExit on all script, of this gameobject, that implement the interface
                PhysicsCollision2D physicsObjectCollision2D = new PhysicsCollision2D(entry.Key,
                                                                                     Collider,
                                                                                     entry.Key.attachedRigidbody,
                                                                                     Rigidbody2D,
                                                                                     entry.Key.transform,
                                                                                     entry.Key.gameObject,
                                                                                     Vector2.zero,
                                                                                     true);

                foreach (IPhysicsObjectCollisionListener collisionListener in _collisionListeners)
                {
                    collisionListener.OnPhysicsObjectCollisionExit(physicsObjectCollision2D);
                }

                physicsObjectCollision2D = new PhysicsCollision2D(Collider,
                                                                  entry.Key,
                                                                  Rigidbody2D,
                                                                  entry.Key.attachedRigidbody,
                                                                  transform,
                                                                  gameObject,
                                                                  Vector2.zero,
                                                                  true);

                foreach (IPhysicsObjectCollisionListener listener in entry.Value)
                {
                    listener.OnPhysicsObjectCollisionExit(physicsObjectCollision2D);
                }
            }
        }
    }

    protected virtual void OnColliderHitCheck(RaycastHit2D hit) { }

    public void AddVerticalVelocity(float addedVelocity)
    {
        Velocity = new Vector2(Velocity.x, Velocity.y + addedVelocity);
    }

    // Check collision exit when the script is disable
    protected virtual void OnDisable()
    {
        CollidingGameObjects.Clear();

        CheckCollisionExit();
    }

    // Reset all movement and related variable when the script is enable
    protected virtual void OnEnable()
    {
        GroundNormal = Vector2.zero;
        IsGrounded = false;
        Velocity = Vector2.zero;
        TargetHorizontalVelocity = .0f;

        PreviouslyCollidingGameObject.Clear();
        CollidingGameObjects.Clear();
    }
}
