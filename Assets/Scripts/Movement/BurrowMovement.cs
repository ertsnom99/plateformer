using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AudioSource))]

public class BurrowMovement : FlyingMovement
{
    [SerializeField]
    private float _rotationSpeed = 10.0f;
    [SerializeField]
    private ParticleSystem _burrowParticles;

    [Header("Sound")]
    [SerializeField]
    private AudioClip _burrowingSound;
    [SerializeField]
    private float _maxSoundVolume = 0.8f;
    [SerializeField]
    private float _minVelocityForMaxSound = 2.5f;

    private Vector2 _movementDirection;
    private bool _hasParticlesStarted = false;

    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();

        _audioSource = GetComponent<AudioSource>();
    }

    protected override void FixedUpdate()
    {
        // Make the movement direction rotate toward the input over time
        Vector2 movementInput = new Vector2(CurrentInputs.Horizontal, CurrentInputs.Vertical);

        if (movementInput.sqrMagnitude > 1.0f)
        {
            movementInput.Normalize();
        }

        Quaternion startRot = Quaternion.LookRotation(_movementDirection, Vector3.forward);
        Quaternion endRot = movementInput.magnitude > .001f ? Quaternion.LookRotation(movementInput.normalized, Vector3.forward) : startRot;
        
        _movementDirection = Quaternion.Slerp(startRot, endRot, _rotationSpeed * Time.fixedDeltaTime) * Vector3.forward;

        Rigidbody.AddForce(_movementDirection * movementInput.magnitude * Speed - Rigidbody.velocity);

        Animate();

        // Play the particule system when moving
        if (!_hasParticlesStarted && Rigidbody.velocity.sqrMagnitude > .1f)
        {
            _burrowParticles.Play();
            _hasParticlesStarted = true;
        }
        else if (_hasParticlesStarted && Rigidbody.velocity.sqrMagnitude < .1f)
        {
            _burrowParticles.Stop();
            _hasParticlesStarted = false;
        }

        // Change the sound volume based on the input
        _audioSource.volume = _maxSoundVolume * Mathf.Clamp01(Rigidbody.velocity.magnitude / _minVelocityForMaxSound);
    }

    public void SetMoveDirection(Vector2 direction)
    {
        _movementDirection = direction;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (_burrowingSound)
        {
            _audioSource.clip = _burrowingSound;
            _audioSource.Play();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        _burrowParticles.Stop();
        _audioSource.Stop();
    }
}
