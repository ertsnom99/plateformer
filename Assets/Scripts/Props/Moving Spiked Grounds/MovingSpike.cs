using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AudioSource))]

public class MovingSpike : SpikeRespawner
{
    [Header("Movement")]
    [SerializeField]
    private Transform _startPosition;
    [SerializeField]
    private Transform _endPosition;
    [SerializeField]
    private float _movementSpeed;

    private Vector3 _movementDirection;

    private bool _isMoving = false;
    private bool _hasReachedEnd = false;

    [Header("Sound")]
    [SerializeField]
    private AudioClip _movingSound;

    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();

        if (!_startPosition)
        {
            Debug.LogError("No start position was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_endPosition)
        {
            Debug.LogError("No end position was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        transform.position = _startPosition.position;
        _movementDirection = (_endPosition.position - _startPosition.position).normalized;

        _audioSource = GetComponent<AudioSource>();

        _audioSource.Stop();

        _audioSource.loop = true;
        _audioSource.clip = _movingSound;
    }

    private void Update()
    {
        if (_isMoving)
        {
            transform.position = transform.position + _movementDirection * _movementSpeed * Time.deltaTime;
            Vector3 positionToEndPosition = (_endPosition.position - transform.position).normalized;
            
            if (positionToEndPosition != _movementDirection)
            {
                transform.position = _endPosition.position;

                _isMoving = false;
                _hasReachedEnd = true;

                _audioSource.Stop();
            }
        }
    }

    protected override void OnTouchSpike(Collider2D col)
    {
        base.OnTouchSpike(col);

        if (col.CompareTag(GameManager.PlayerTag) || col.CompareTag(GameManager.EnemyTag))
        {
            PauseMovement();
        }
    }

    public void StartMoving()
    {
        if (!_isMoving && !_hasReachedEnd)
        {
            _isMoving = true;

            _audioSource.Play();
        }
    }

    public void PauseMovement()
    {
        _isMoving = false;

        _audioSource.Pause();
    }

    public void ResumeMovement()
    {
        if (!_hasReachedEnd)
        {
            _isMoving = true;

            _audioSource.Play();
        }
    }

    public void ResetMovement()
    {
        transform.position = _startPosition.position;

        _isMoving = false;
        _hasReachedEnd = false;

        _audioSource.Stop();
    }

    public void SetToEndPosition()
    {
        transform.position = _endPosition.position;
    }
}
