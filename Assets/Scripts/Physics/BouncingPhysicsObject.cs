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

public class BouncingPhysicsObject : SubscribablePhysicsObject<IBouncingPhysicsObjectSubscriber>, IPhysicsObjectCollisionListener
{
    enum BounceStopCondition { BounceDurationElapsed, MaxBounceCountReached, UnderMinVelocity };

    [Header("Bounce")]
    [SerializeField]
    private float _bounciness = 1.0f;
    [SerializeField]
    private float _bounceFreezeDuration = .0f;
    [SerializeField]
    private int _bounceMaxTry = 100;
    [SerializeField]
    private bool _bounceHasStopCondition = true;
    [SerializeField]
    private bool _freezeWhenStopConditionReached = false;

    [Header("Sound")]
    [SerializeField]
    private AudioClip _bounceSound;
    [SerializeField]
    private bool _playBounceSoundOnLastBounce = false;

    [Header("Condition")]
    [SerializeField]
    private BounceStopCondition _bounceStopCondition;
    [SerializeField]
    private float _bounceDuration = 10.0f;
    private float _bounceElapsedTime = .0f;
    [SerializeField]
    private int _maxBounceCount = 40;
    [SerializeField]
    private float _minVelocity = 5.0f;

    private bool _movementFrozen = true;

    public bool MovementFrozen
    {
        get { return _movementFrozen; }
        private set { _movementFrozen = value; }
    }

    private int _bounceCount = 0;

    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();

        _audioSource = GetComponent<AudioSource>();

        FreezeMovement(true);
    }

    public void Launch(Vector2 force)
    {
        _bounceElapsedTime = .0f;
        _bounceCount = 0;

        FreezeMovement(false);
        Velocity = force;

        // Tell subscribers that the bounce started
        foreach (IBouncingPhysicsObjectSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyBounceStarted();
        }
    }

    protected override void Update()
    {
        if (!MovementFrozen)
        {
            _bounceElapsedTime += Time.deltaTime;

            if (_bounceHasStopCondition)
            {
                if (_bounceStopCondition == BounceStopCondition.BounceDurationElapsed && _bounceElapsedTime >= _bounceDuration)
                {
                    if (_freezeWhenStopConditionReached)
                    {
                        FreezeMovement(true);
                    }
                    
                    // Tell subscribers that the bounce finished
                    foreach (IBouncingPhysicsObjectSubscriber subscriber in Subscribers)
                    {
                        subscriber.NotifyBounceFinished();
                    }
                }
                else if (_bounceStopCondition == BounceStopCondition.UnderMinVelocity && Velocity.magnitude < _minVelocity)
                {
                    if (_freezeWhenStopConditionReached)
                    {
                        FreezeMovement(true);
                    }

                    // Tell subscribers that the bounce finished
                    foreach (IBouncingPhysicsObjectSubscriber subscriber in Subscribers)
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
            PreviouslyCollidingGameObject = new Dictionary<Collider2D, IPhysicsObjectCollisionListener[]>(CollidingGameObjects);
            CollidingGameObjects.Clear();

            // Move the object
            Vector2 movement = Velocity * Time.fixedDeltaTime;
            Move(movement);

            // Check and call collision exit methods
            CheckCollisionExit();

            if (DebugVelocity)
            {
                Debug.DrawLine(transform.position, transform.position + (Vector3)Velocity, Color.blue);
                Debug.DrawLine(transform.position, transform.position + (Vector3)movement, Color.red);
                Debug.DrawLine(transform.position, transform.position + Vector3.up * ShellRadius, Color.yellow);
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
        while (!MovementFrozen && tryCount <= _bounceMaxTry && distanceToTravel > .0f)
        {
            // Consider that the objet can travel all the remining distance without hitting anything
            distanceBeforeHit = distanceToTravel;

            // Stock the direction of velocity since it is changed when there is a collision
            currentVelocityDirection = Velocity.normalized;

            // Check for collision only if the object moves enough
            if (distanceToTravel > MinMoveDistance)
            {
                // Cast and only consider the first collision
                int count = Rigidbody2D.Cast(Velocity.normalized, ContactFilter, HitBuffer, distanceToTravel + ShellRadius);

                if (count > 0 && HitBuffer[0])
                {
                    // Update the velocity and the number of bounce
                    OnHit(HitBuffer[0].normal);

                    if (!MovementFrozen)
                    {
                        // Stop all movement for a moment
                        StartCoroutine(FreezeMovementOnBounce());
                    }

                    // Check and call collision enter methods
                    CheckCollisionEnter(HitBuffer[0]);

                    // Update how much distance can be done before hitting something  
                    distanceBeforeHit = HitBuffer[0].distance - ShellRadius;

                    // Will allow inheriting classes to add logic during the hit checks
                    OnColliderHitCheck(HitBuffer[0]);
                }
            }

            distanceToTravel -= distanceBeforeHit;

            // Apply the movement
            Rigidbody2D.position = Rigidbody2D.position + currentVelocityDirection * distanceBeforeHit;

            tryCount++;
        }

        // Rotate to be oriented toward Velocity direction
        float angle = Mathf.Atan2(Velocity.y, Velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnHit(Vector2 normal)
    {
        // Reflect the direction of the velocity along the normal
        Velocity = Velocity.magnitude * _bounciness * Vector2.Reflect(Velocity.normalized, normal).normalized;

        // Increment and check the number of bounce
        _bounceCount++;

        _audioSource.PlayOneShot(_bounceSound);
        
        if (_bounceHasStopCondition && _bounceStopCondition == BounceStopCondition.MaxBounceCountReached && _bounceCount >= _maxBounceCount)
        {
            if (_freezeWhenStopConditionReached)
            {
                FreezeMovement(true);
            }

            if (!_playBounceSoundOnLastBounce)
            {
                _audioSource.Stop();
            }

            // Tell subscribers that the bounce finished
            foreach (IBouncingPhysicsObjectSubscriber subscriber in Subscribers)
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

        yield return new WaitForSeconds(_bounceFreezeDuration);

        FreezeMovement(false);
        OnFreezeEnd();
    }

    protected virtual void OnFreezeStart() { }

    protected virtual void OnFreezeEnd() { }

    private void OnDisable()
    {
        FreezeMovement(true);
    }

    // Methods of the IPhysicsObjectCollisionListener interface
    public void OnPhysicsObjectCollisionExit(PhysicsCollision2D collision) { }

    public void OnPhysicsObjectCollisionEnter(PhysicsCollision2D collision)
    {
        if (!MovementFrozen)
        {
            OnHit(collision.Normal);
        }
    }
}
