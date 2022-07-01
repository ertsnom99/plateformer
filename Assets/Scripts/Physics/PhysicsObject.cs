/*
 * Based on: https://unity3d.com/fr/learn/tutorials/topics/2d-game-creation/player-controller-script 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script requires those components and will be added if they aren't already there
[RequireComponent(typeof(Rigidbody2D))]

public class PhysicsObject : MonoBehaviour
{
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
    protected Vector2 GroundNormal = Vector2.up;
    //protected float m_groundAngle;

    public bool IsGrounded { get; private set; }
    
    [SerializeField]
    protected bool DebugVelocity = false;

    public Vector2 Velocity { get; protected set; }

    protected float TargetHorizontalVelocity;
    protected ContactFilter2D ContactFilter = new ContactFilter2D();
    protected RaycastHit2D[] HitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> MoveHitBufferList = new List<RaycastHit2D>(16);
    protected List<RaycastHit2D> AllHitBufferList = new List<RaycastHit2D>(16);

    protected struct CollidingInfo
    {
        public RaycastHit2D Hit;
        public IPhysicsCollision2DListener[] Listeners;
    }

    // The IPhysicsCollision2DListener[] are those from the other hit gameobject (we already hold an array of the IPhysicsCollision2DListener for this gameobject)
    protected Dictionary<Collider2D, CollidingInfo> PreviouslyCollidingGameObject = new Dictionary<Collider2D, CollidingInfo>();
    protected Dictionary<Collider2D, CollidingInfo> CollidingGameObjects = new Dictionary<Collider2D, CollidingInfo>();

    private IPhysicsCollision2DListener[] _collisionListeners;

    protected Collider2D Collider;
    protected Rigidbody2D Rigidbody2D;

    protected const float MinMoveDistance = 0.001f;

    protected virtual void Awake()
    {
        ContactFilter.useLayerMask = true;
        ContactFilter.useTriggers = false;

        CurrentGravityModifier = GravityModifier;

        IsGrounded = false;

        _collisionListeners = GetComponentsInChildren<IPhysicsCollision2DListener>();

        Collider = GetComponent<Collider2D>();
        Rigidbody2D = GetComponent<Rigidbody2D>();

        if (!Collider)
        {
            Debug.LogError("No collider was set for " + GetType() + " script of " + gameObject.name + "!");
        }
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
        IsGrounded = false;
        AllHitBufferList.Clear();

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
        
        // Create a Vector perpendicular to the normal
        Vector2 movementAlongGround = new Vector2(GroundNormal.y, -GroundNormal.x);

        // The X movement is executed first, then the Y movement is executed. This allows a better control of each type of movement and helps to avoid
        // corner cases. This technique was used in the 16 bit era.
        Vector2 deltaPosition = Velocity * Time.fixedDeltaTime;

        Vector2 movement = movementAlongGround * deltaPosition.x;
        Move(movement, false);

        Vector2 totalMovement = movement;
        movement = Vector2.up * deltaPosition.y;
        Move(movement, true);

        totalMovement += movement;
        StartCoroutine(CallCollisionEvents(totalMovement));

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

    protected void Move(Vector2 movement, bool yMovement)
    {
        float distance = movement.magnitude;
        
        // Check for collision only if the object moves enough
        if (distance > MinMoveDistance)
        {
            // Tells to use the layer settings from the Physics2D settings (the matrix)
            ContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));

            int count = Collider.Cast(movement, ContactFilter, HitBuffer, distance + ShellRadius);
            MoveHitBufferList.Clear();
            
            // Transfer hits to MoveHitBufferList
            // The hit buffers have always the same number of elements in it, but some might be null.
            // The purpose of the transfer is to only keep the actual result of the cast
            for (int i = 0; i < count; i++)
            {
                MoveHitBufferList.Add(HitBuffer[i]);

                if (!AllHitBufferList.Contains(HitBuffer[i]))
                {
                    AllHitBufferList.Add(HitBuffer[i]);
                }
            }

            foreach (RaycastHit2D hit in MoveHitBufferList)
            {
                Vector2 currentNormal = hit.normal;

                // Check if the object is grounded
                if (yMovement && currentNormal.y >= _minGroundNormalY)
                {
                    IsGrounded = true;

                    // Update ground normal
                    GroundNormal = currentNormal;
                    currentNormal.x = 0;
                }
                
                Vector2 velocityUsed = yMovement ? Vector2.up * Velocity.y : Vector2.right * Velocity.x;
                // Check how much of the velocity is responsible for going to go throw other colliders
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
                }

                // Calculate how much movement can be done, before hitting something, considering the ShellRadius  
                float modifiedDistance = hit.distance - ShellRadius;

                // If the object should move less then what was thought at first, then move less
                distance = modifiedDistance < distance ? modifiedDistance : distance;

                // Will allow inheriting classes to add logic during the hit checks
                OnColliderHitCheck(hit);
            }
        }

        // Apply the movement
        Rigidbody2D.position = Rigidbody2D.position + movement.normalized * distance;
    }

    #region Collision methods
    protected IEnumerator CallCollisionEvents(Vector2 totalMovement)
    {
        // Make sure to call collision events after the OnCollisionXXX event functions of Unity
        yield return new WaitForFixedUpdate();

        // Backup the list of gameObjects that used to collide and clear the original
        PreviouslyCollidingGameObject = new Dictionary<Collider2D, CollidingInfo>(CollidingGameObjects);
        CollidingGameObjects.Clear();

        // Check and call collision methods
        foreach (RaycastHit2D hit in AllHitBufferList)
        {
            CheckCollisionHit(hit);
        }

        CheckPreviousCollidingGameObject(totalMovement);
    }

    protected void CheckCollisionHit(RaycastHit2D hit)
    {
        Collider2D hitCollider = hit.collider;

        // Call OnPhysicsObjectCollisionEnter on all script, of this gameobject, that implement the interface
        Vector2 relativeVelocity = hit.rigidbody ? hit.rigidbody.velocity - Velocity : -Velocity;

        PhysicsCollision2D physicsObjectCollision2D = new PhysicsCollision2D(hitCollider,
                                                                             Collider,
                                                                             hit.rigidbody,
                                                                             Rigidbody2D,
                                                                             hit.transform,
                                                                             hitCollider.gameObject,
                                                                             relativeVelocity,
                                                                             true,
                                                                             hit.point,
                                                                             hit.normal);
        
        foreach (IPhysicsCollision2DListener collisionListener in _collisionListeners)
        {
            // If the hit gameObject wasn't previously hit
            if (!PreviouslyCollidingGameObject.ContainsKey(hitCollider))
            {
                collisionListener.OnPhysicsCollision2DEnter(physicsObjectCollision2D);
            }

            collisionListener.OnPhysicsCollision2DStay(physicsObjectCollision2D);
        }

        // Call OnPhysicsObjectCollisionEnter on all script, of the hit gameobject, that implement the interface
        IPhysicsCollision2DListener[] collisionListeners = hitCollider.GetComponents<IPhysicsCollision2DListener>();

        if (collisionListeners.Length > 0)
        {
            relativeVelocity = hit.rigidbody ? Velocity - hit.rigidbody.velocity : Velocity;

            physicsObjectCollision2D = new PhysicsCollision2D(Collider,
                                                              hitCollider,
                                                              Rigidbody2D,
                                                              hit.rigidbody,
                                                              transform,
                                                              gameObject,
                                                              relativeVelocity,
                                                              true,
                                                              hit.point,
                                                              -hit.normal);

            foreach (IPhysicsCollision2DListener listener in collisionListeners)
            {
                // If the hit gameObject wasn't previously hit
                if (!PreviouslyCollidingGameObject.ContainsKey(hitCollider))
                {
                    listener.OnPhysicsCollision2DEnter(physicsObjectCollision2D);
                }

                listener.OnPhysicsCollision2DStay(physicsObjectCollision2D);
            }
        }

        // Update the list of currently colliding gameObject
        if (!CollidingGameObjects.ContainsKey(hitCollider))
        {
            CollidingGameObjects.Add(hitCollider, new CollidingInfo { Hit = hit, Listeners = collisionListeners });
            //if (hitCollider.gameObject.name == "DrillableSurface") Debug.Log("Added " + hitCollider.gameObject.name + "    Now there is " + CollidingGameObjects.Count);
        }

        if (PreviouslyCollidingGameObject.ContainsKey(hitCollider))
        {
            PreviouslyCollidingGameObject.Remove(hitCollider);
        }
    }

    protected void CheckPreviousCollidingGameObject(Vector2 totalMovement, bool forceExitAll = false)
    {
        RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

        // Can't directly call CheckCollisionHit while going threw PreviouslyCollidingGameObject, therefor must cache hit colliders
        Dictionary<Collider2D, RaycastHit2D> hitCollider = new Dictionary<Collider2D, RaycastHit2D>();
        
        // Base on the normal and last movement, check if remaining previously colliding gameobject is in contact
        foreach (KeyValuePair<Collider2D, CollidingInfo> entry in PreviouslyCollidingGameObject)
        {
            bool shouldExit = forceExitAll;
            Collider2D collider = entry.Key;
            
            if (!shouldExit)
            {
                shouldExit = true;

                // Check if the object is still touching by doing a cast toward the surface
                int count = Collider.Cast(-entry.Value.Hit.normal, ContactFilter, hitBuffer, MinMoveDistance + ShellRadius);

                for (int i = 0; i < count; i++)
                {
                    if (hitBuffer[i].collider == collider)
                    {
                        shouldExit = false;
                        break;
                    }
                }
            }

            if (shouldExit)
            {
                // Call OnPhysicsObjectCollisionExit on all script, of this gameobject, that implement the interface
                PhysicsCollision2D physicsObjectCollision2D = new PhysicsCollision2D(collider,
                                                                                        Collider,
                                                                                        collider.attachedRigidbody,
                                                                                        Rigidbody2D,
                                                                                        collider.transform,
                                                                                        collider.gameObject,
                                                                                        Vector2.zero,
                                                                                        true);

                foreach (IPhysicsCollision2DListener collisionListener in _collisionListeners)
                {
                    collisionListener.OnPhysicsCollision2DExit(physicsObjectCollision2D);
                }

                physicsObjectCollision2D = new PhysicsCollision2D(Collider,
                                                                    collider,
                                                                    Rigidbody2D,
                                                                    collider.attachedRigidbody,
                                                                    transform,
                                                                    gameObject,
                                                                    Vector2.zero,
                                                                    true);

                foreach (IPhysicsCollision2DListener listener in entry.Value.Listeners)
                {
                    listener.OnPhysicsCollision2DExit(physicsObjectCollision2D);
                }
            }
            else
            {
                hitCollider.Add(entry.Key, entry.Value.Hit);
            }
        }

        // Do collision for cached hits
        foreach (KeyValuePair<Collider2D, RaycastHit2D> entry in hitCollider)
        {
            CheckCollisionHit(entry.Value);
        }
    }

    protected virtual void OnColliderHitCheck(RaycastHit2D hit) { }
    #endregion

    public void AddVerticalVelocity(float addedVelocity)
    {
        Velocity = new Vector2(Velocity.x, Velocity.y + addedVelocity);
    }

    // Check collision exit when the script is disable
    protected virtual void OnDisable()
    {
        CollidingGameObjects.Clear();

        CheckPreviousCollidingGameObject(Vector3.zero, true);
    }

    // Reset all movement and related variable when the script is enable
    protected virtual void OnEnable()
    {
        GroundNormal = Vector2.up;
        IsGrounded = false;
        Velocity = Vector2.zero;
        TargetHorizontalVelocity = .0f;
        
        PreviouslyCollidingGameObject.Clear();
        CollidingGameObjects.Clear();

        InitialiseRigidbody2D();
    }
}
