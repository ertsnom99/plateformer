using System.Collections;
using UnityEngine;

public interface IUnityBouncingPhysicsObjectSubscriber
{
    void NotifyBounceStarted();
    void NotifyBounceFinished();
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]

public class UnityBouncingPhysicsObject : MonoSubscribable<IUnityBouncingPhysicsObjectSubscriber>
{
    enum BounceStopCondition{ BounceDurationElapsed, MaxBounceCountReached, UnderMinVelocity };

    [Header("Bounce")]
    [SerializeField]
    private float _bounceFreezeDuration = .1f;
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
    private float _bounceDuration = 3.0f;
    private float _bounceElapsedTime = .0f;
    [SerializeField]
    private int _maxBounceCount = 10;
    [SerializeField]
    private float _minVelocity = 5.0f;

    private bool _collisionHappened = false;
    private Vector2 _velocityAtFreeze;
    private int _bounceCount = 0;

    private bool _movementFrozen = false;

    public bool MovementFrozen
    {
        get { return _movementFrozen; }
        private set { _movementFrozen = value; }
    }

    private Rigidbody2D _rigidbody2D;
    private AudioSource _audioSource;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        InitialiseRigidbody2D();

        _audioSource = GetComponent<AudioSource>();

        FreezeMovement(true);
    }

    private void InitialiseRigidbody2D()
    {
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody2D.simulated = true;
        _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rigidbody2D.sleepMode = RigidbodySleepMode2D.NeverSleep;
        _rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void Launch(Vector2 force)
    {
        _bounceElapsedTime = .0f;
        _bounceCount = 0;

        FreezeMovement(false);
        _rigidbody2D.AddForce(force, ForceMode2D.Impulse);

        // Tell subscribers that the bounce started
        foreach (IUnityBouncingPhysicsObjectSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyBounceStarted();
        }
    }

    private void Update()
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
                    foreach (IUnityBouncingPhysicsObjectSubscriber subscriber in Subscribers)
                    {
                        subscriber.NotifyBounceFinished();
                    }
                }
                else if (_bounceStopCondition == BounceStopCondition.UnderMinVelocity && _rigidbody2D.velocity.magnitude < _minVelocity)
                {
                    if (_freezeWhenStopConditionReached)
                    {
                        FreezeMovement(true);
                    }

                    // Tell subscribers that the bounce finished
                    foreach (IUnityBouncingPhysicsObjectSubscriber subscriber in Subscribers)
                    {
                        subscriber.NotifyBounceFinished();
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!MovementFrozen)
        {
            _collisionHappened = false;

            // Rotate to be oriented toward Velocity direction
            float angle = Mathf.Atan2(_rigidbody2D.velocity.y, _rigidbody2D.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!MovementFrozen && !_collisionHappened)
        {
            _collisionHappened = true;
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
                foreach (IUnityBouncingPhysicsObjectSubscriber subscriber in Subscribers)
                {
                    subscriber.NotifyBounceFinished();
                }
            }
            else if (!MovementFrozen)
            {
                StartCoroutine(FreezeForDuration());
            }
        }
    }

    public void FreezeMovement(bool freeze)
    {
        if (freeze)
        {
            _rigidbody2D.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
            _velocityAtFreeze = _rigidbody2D.velocity;
        }
        else
        {
            _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rigidbody2D.velocity = _velocityAtFreeze;
        }

        MovementFrozen = freeze;
    }

    private IEnumerator FreezeForDuration()
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
}
