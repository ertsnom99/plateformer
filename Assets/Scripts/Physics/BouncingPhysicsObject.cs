using System.Collections;
using UnityEngine;

public interface IBouncingPhysicsObjectSubscriber
{
    void NotifyBounceStarted();
    void NotifyBounceFinished();
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AudioSource))]

public class BouncingPhysicsObject : SubscribablePhysicsObject<IBouncingPhysicsObjectSubscriber>, IPhysicsCollision2DListener
{
    enum BounceStopCondition { BounceDurationElapsed, MaxBounceCountReached, UnderMinVelocity };
    
    [Header("Bounce")]
    [SerializeField]
    private float _bounciness = 1.0f;
    [SerializeField]
    private float _bounceFreezeDuration = .0f;
    [SerializeField]
    protected int BounceMaxTry = 100;
    [SerializeField]
    private bool _bounceHasStopCondition = true;
    [SerializeField]
    private bool _freezeWhenStopConditionReached = false;

    [Header("Sound")]
    [SerializeField]
    private AudioClip _bounceSound;

    private int _lastBounceSoundPlayedFrame = -1;

    [Header("Bounce Condition")]
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

    protected AudioSource AudioSource;

    protected override void Awake()
    {
        base.Awake();
        
        AudioSource = GetComponent<AudioSource>();

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
            AllHitBufferList.Clear();

            // Apply gravity
            Velocity += CurrentGravityModifier * Physics2D.gravity * Time.fixedDeltaTime;

            // Move the object
            Vector2 movement = Velocity * Time.fixedDeltaTime;
            Move(movement);

            StartCoroutine(CallCollisionEvents());

            if (DebugVelocity)
            {
                Debug.DrawLine(transform.position, transform.position + (Vector3)Velocity, Color.blue);
                Debug.DrawLine(transform.position, transform.position + (Vector3)movement, Color.red);
                Debug.DrawLine(transform.position, transform.position + Vector3.up * ShellRadius, Color.yellow);
                Debug.Log(Velocity);
            }
        }
    }

    protected virtual void Move(Vector2 movement)
    {
        int tryCount = 0;
        float distanceToTravel = movement.magnitude;
        float distanceBeforeHit = movement.magnitude;
        Vector2 currentVelocityDirection = Velocity.normalized;

        // Keeps trying to move until the object travelled the necessary distance
        while (!MovementFrozen && tryCount <= BounceMaxTry && distanceToTravel > .0f)
        {
            // Consider that the objet can travel all the remining distance without hitting anything
            distanceBeforeHit = distanceToTravel;

            // Stock the direction of velocity since it is changed when there is a collision
            currentVelocityDirection = Velocity.normalized;

            // Check for collision only if the object moves enough
            if (distanceToTravel > MinMoveDistance)
            {
                // Cast and only consider the first collision
                int count = Collider.Cast(Velocity.normalized, ContactFilter, HitBuffer, distanceToTravel + ShellRadius);

                if (count > 0 && HitBuffer[0])
                {
                    if (!AllHitBufferList.Contains(HitBuffer[0]))
                    {
                        AllHitBufferList.Add(HitBuffer[0]);
                    }

                    // Update the velocity and the number of bounce
                    OnBounce(HitBuffer[0].normal);

                    if (!MovementFrozen)
                    {
                        // Stop all movement for a moment
                        StartCoroutine(FreezeMovementOnBounce());
                    }

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

    protected void OnBounce(Vector2 hitNormal)
    {
        // Reflect the direction of the velocity along the normal
        Velocity = Velocity.magnitude * _bounciness * Vector2.Reflect(Velocity.normalized, hitNormal).normalized;

        // Increment and check the number of bounce
        _bounceCount++;

        // Avoid playing the same sound many times during the same frame
        if (Time.frameCount != _lastBounceSoundPlayedFrame)
        {
            AudioSource.PlayOneShot(_bounceSound);

            _lastBounceSoundPlayedFrame = Time.frameCount;
        }
        
        if (_bounceHasStopCondition && _bounceStopCondition == BounceStopCondition.MaxBounceCountReached && _bounceCount >= _maxBounceCount)
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

    public void FreezeMovement(bool freeze)
    {
        MovementFrozen = freeze;
    }

    protected IEnumerator FreezeMovementOnBounce()
    {
        FreezeMovement(true);
        OnFreezeStart();

        yield return new WaitForSeconds(_bounceFreezeDuration);

        FreezeMovement(false);
        OnFreezeEnd();
    }

    protected virtual void OnFreezeStart() { }

    protected virtual void OnFreezeEnd() { }

    protected override void OnDisable()
    {
        base.OnDisable();

        FreezeMovement(true);
    }

    // Methods of the IPhysicsObjectCollisionListener interface
    public void OnPhysicsCollision2DEnter(PhysicsCollision2D collision)
    {
        if (!MovementFrozen)
        {
            OnBounce(collision.Normal);
        }
    }

    public void OnPhysicsCollision2DStay(PhysicsCollision2D collision) { }

    public void OnPhysicsCollision2DExit(PhysicsCollision2D collision) { }
}
