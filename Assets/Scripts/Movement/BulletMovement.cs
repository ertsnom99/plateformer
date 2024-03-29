﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Rigidbody2D))]

// TODO: PhysicsObject was updated for better detection of collision, this wasn't. Update this if used!!!
public class BulletMovement : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField]
    protected float ShellRadius = 0.05f;
    [SerializeField]
    private float _rotationSpeed = 2.0f;

    private Vector2 _aimingDirection;

    [Header("Movement")]
    [SerializeField]
    private float _speed = 5.0f;

    private Vector2 _velocity;

    [Header("Collision")]
    [SerializeField]
    private Collider2D _collider;
    [SerializeField]
    private bool _updateContactFilterLayerMask = false;

    protected ContactFilter2D ContactFilter;
    protected RaycastHit2D[] HitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> HitBufferList = new List<RaycastHit2D>(16);

    protected Dictionary<Collider2D, IPhysicsCollision2DListener[]> PreviouslyCollidingGameObject = new Dictionary<Collider2D, IPhysicsCollision2DListener[]>();
    protected Dictionary<Collider2D, IPhysicsCollision2DListener[]> CollidingGameObjects = new Dictionary<Collider2D, IPhysicsCollision2DListener[]>();

    private IPhysicsCollision2DListener[] _collisionListeners;

    private Rigidbody2D _rigidbody2D;

    private void Awake()
    {
        // Tells to use the layer settings from the Physics2D settings (the matrix)
        ContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        ContactFilter.useLayerMask = true;
        ContactFilter.useTriggers = false;

        _collisionListeners = GetComponentsInChildren<IPhysicsCollision2DListener>();

        if (!_collider)
        {
            Debug.LogError("No collider was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        _aimingDirection = transform.right;
        _velocity = (Vector2)transform.right.normalized * _speed;

        _rigidbody2D = GetComponent<Rigidbody2D>();

        InitialiseRigidbody2D();
    }

    private void InitialiseRigidbody2D()
    {
        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D.simulated = true;
        _rigidbody2D.useFullKinematicContacts = true;
        _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rigidbody2D.sleepMode = RigidbodySleepMode2D.NeverSleep;
        _rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void SetAimingDirection(Vector2 direction)
    {
        _aimingDirection = direction.normalized;
    }

    private void FixedUpdate()
    {
        UpdateRotation();
        UpdateMovement();
    }

    private void UpdateRotation()
    {
        // Apply the rotation
        float angle = Mathf.Atan2(_aimingDirection.y, _aimingDirection.x) * Mathf.Rad2Deg;
        _rigidbody2D.rotation = Mathf.LerpAngle(transform.rotation.eulerAngles.z, angle, _rotationSpeed * Time.fixedDeltaTime);
    }

    private void UpdateMovement()
    {
        if (_updateContactFilterLayerMask)
        {
            ContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        }

        HitBufferList.Clear();

        // Calculate Velocity
        _velocity = (Vector2)transform.right.normalized * _speed;

        // Calculate movement
        Vector2 movement = _velocity * Time.fixedDeltaTime;

        float distance = movement.magnitude;

        // Test collision
        int count = _collider.Cast(movement, ContactFilter, HitBuffer, distance + ShellRadius);

        // Transfer hits to m_hitBufferList
        // m_hitBuffer has always the same number of elements in it, but some might be null.
        // The purpose of the transfer is to only keep the actual result of the cast
        for (int i = 0; i < count; i++)
        {
            HitBufferList.Add(HitBuffer[i]);
        }

        foreach (RaycastHit2D hit in HitBufferList)
        {
            // Calculate how much movement can be done, before hitting something, considering the ShellRadius  
            float modifiedDistance = hit.distance - ShellRadius;

            // If the object should move less then what was tought at first, then move less
            distance = modifiedDistance < distance ? modifiedDistance : distance;
        }
        
        // Apply the movement
        _rigidbody2D.position = _rigidbody2D.position + movement.normalized * distance;
        
        StartCoroutine(CallCollisionEvents());
    }

    protected IEnumerator CallCollisionEvents()
    {
        // Make sure to call collision events after the OnCollisionXXX event functions of Unity
        yield return new WaitForFixedUpdate();

        // Backup the list of gameObjects that used to collide and clear the original
        PreviouslyCollidingGameObject = new Dictionary<Collider2D, IPhysicsCollision2DListener[]>(CollidingGameObjects);
        CollidingGameObjects.Clear();

        // Check and call collision enter methods
        foreach (RaycastHit2D hit in HitBufferList)
        {
            CheckCollisionEnterAndStay(hit);
        }

        // Check and call collision exit methods
        CheckCollisionExit();
    }
    
    protected void CheckCollisionEnterAndStay(RaycastHit2D hit)
    {
        // Call OnPhysicsObjectCollisionEnter on all script, of this gameobject, that implement the interface
        Vector2 relativeVelocity = hit.rigidbody ? hit.rigidbody.velocity - _velocity : -_velocity;

        PhysicsCollision2D physicsObjectCollision2D = new PhysicsCollision2D(hit.collider,
                                                                             _collider,
                                                                             hit.rigidbody,
                                                                             _rigidbody2D,
                                                                             hit.transform,
                                                                             hit.collider.gameObject,
                                                                             relativeVelocity,
                                                                             true,
                                                                             hit.point,
                                                                             hit.normal);

        foreach (IPhysicsCollision2DListener collisionListener in _collisionListeners)
        {
            // If the hit gameObject wasn't previously hit
            if (!PreviouslyCollidingGameObject.ContainsKey(hit.collider))
            {
                collisionListener.OnPhysicsCollision2DEnter(physicsObjectCollision2D);
            }

            collisionListener.OnPhysicsCollision2DStay(physicsObjectCollision2D);
        }

        // Call OnPhysicsObjectCollisionEnter on all script, of the hit gameobject, that implement the interface
        IPhysicsCollision2DListener[] collisionListeners = hit.collider.GetComponents<IPhysicsCollision2DListener>();

        if (collisionListeners.Length > 0)
        {
            relativeVelocity = hit.rigidbody ? _velocity - hit.rigidbody.velocity : _velocity;

            physicsObjectCollision2D = new PhysicsCollision2D(_collider,
                                                              hit.collider,
                                                              _rigidbody2D,
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
                if (!PreviouslyCollidingGameObject.ContainsKey(hit.collider))
                {
                    listener.OnPhysicsCollision2DEnter(physicsObjectCollision2D);
                }

                listener.OnPhysicsCollision2DStay(physicsObjectCollision2D);
            }
        }

        // Update the list of currently colliding gameObject
        if (!CollidingGameObjects.ContainsKey(hit.collider))
        {
            CollidingGameObjects.Add(hit.collider, collisionListeners);
        }
    }

    private void CheckCollisionExit()
    {
        // If a gameObject isn't hit anymore
        foreach (KeyValuePair<Collider2D, IPhysicsCollision2DListener[]> entry in PreviouslyCollidingGameObject)
        {
            // Call OnBulletMovementCollisionExit on all script, of the hit gameobject, that implement the interface
            if (entry.Key && !CollidingGameObjects.ContainsKey(entry.Key))
            {
                // Call OnPhysicsObjectCollisionExit on all script, of this gameobject, that implement the interface
                PhysicsCollision2D bulletMovementCollision2D = new PhysicsCollision2D(entry.Key,
                                                                                     _collider,
                                                                                     entry.Key.attachedRigidbody,
                                                                                     _rigidbody2D,
                                                                                     entry.Key.transform,
                                                                                     entry.Key.gameObject,
                                                                                     Vector2.zero,
                                                                                     true);

                foreach (IPhysicsCollision2DListener collisionListener in _collisionListeners)
                {
                    collisionListener.OnPhysicsCollision2DExit(bulletMovementCollision2D);
                }

                bulletMovementCollision2D = new PhysicsCollision2D(_collider,
                                                                  entry.Key,
                                                                  _rigidbody2D,
                                                                  entry.Key.attachedRigidbody,
                                                                  transform,
                                                                  gameObject,
                                                                  Vector2.zero,
                                                                  true);

                foreach (IPhysicsCollision2DListener listener in entry.Value)
                {
                    listener.OnPhysicsCollision2DExit(bulletMovementCollision2D);
                }
            }
        }
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
        PreviouslyCollidingGameObject.Clear();
        CollidingGameObjects.Clear();
    }
}
