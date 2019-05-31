using Cinemachine;
using Pathfinding;
using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public abstract class AIController : CharacterController
{
    [Header("Possession")]
    [SerializeField]
    private bool _isPossessable = true;

    public bool IsPossessable
    {
        get { return _isPossessable; }
        private set { _isPossessable = value; }
    }

    public bool IsPossessed { get; private set; }

    [SerializeField]
    private CinemachineVirtualCamera _possessionVirtualCamera;

    [SerializeField]
    private AudioClip _onPossessSound;
    [SerializeField]
    private AudioClip _onUnpossessSound;

    [SerializeField]
    private Collider2D _leftPlayerSpawn;
    private ContactFilter2D _leftPlayerSpawnContactFilter;
    [SerializeField]
    private Collider2D _rightPlayerSpawn;
    private ContactFilter2D _rightPlayerSpawnContactFilter;

    private Collider2D[] _overlapResults = new Collider2D[4];
    
    [SerializeField]
    private bool _flipPlayerSpawn = false;

    private Possession _possessingScript;

    [Header("Target")]
    [SerializeField]
    protected Transform Target;
    [SerializeField]
    private float _distanceToDetect = 10.0f;
    [SerializeField]
    protected float StopDistanceToTarget = .4f;
    [SerializeField]
    protected float MinDistanceForTargetReachable = 4.0f;

    public bool HasDetectedTarget { get; private set; }

    [Header("Update")]
    [SerializeField]
    protected int UpdateRate = 5;
    [SerializeField]
    protected float MinDistanceToChangeWaypoint = 0.5f;
    [SerializeField]
    protected bool StopWhenUnreachable = true;
    
    [Header("Debug")]
    [SerializeField]
    private bool _drawDistance = false;
    [SerializeField]
    private bool _logPathFailedError = false;

    protected Path Path;
    protected int TargetWaypoint = 0;

    protected const string _possessedModeAnimationLayerName = "Possessed Mode";
    private int _possessedModeAnimationLayerIndex;

    protected SpriteRenderer SpriteRenderer;
    private Animator _animator;
    protected AudioSource AudioSource;
    protected Seeker Seeker;

    protected virtual void Awake()
    {
        // Tells to use the layer settings from the Physics2D settings (the matrix)
        _leftPlayerSpawnContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(_leftPlayerSpawn.gameObject.layer));
        _leftPlayerSpawnContactFilter.useLayerMask = true;
        _leftPlayerSpawnContactFilter.useTriggers = false;

        _rightPlayerSpawnContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(_rightPlayerSpawn.gameObject.layer));
        _rightPlayerSpawnContactFilter.useLayerMask = true;
        _rightPlayerSpawnContactFilter.useTriggers = false;

        IsPossessed = false;
        HasDetectedTarget = false;

        SpriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();
        Seeker = GetComponent<Seeker>();

        _possessedModeAnimationLayerIndex = _animator.GetLayerIndex(_possessedModeAnimationLayerName);
        _animator.SetLayerWeight(_possessedModeAnimationLayerIndex, .0f);

        if (!_leftPlayerSpawn)
        {
            Debug.LogError("No left player spawn was set!");
        }

        if (!_rightPlayerSpawn)
        {
            Debug.LogError("No right player spawn was set!");
        }
    }

    protected virtual void Start()
    {
        if (!VirtualCameraManager.Instance)
        {
            Debug.LogError("Couldn't find an instance of VirtualCameraManager!");
        }

        if (Target)
        {
            StartCoroutine(UpdatePath());
        }
    }

    protected virtual void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > .0f)
        {
            if (IsPossessed)
            {
                OnUpdatePossessed();
            }
            else
            {
                OnUpdateNotPossessed();
            }
        }
    }

    protected virtual void OnUpdatePossessed()
    {
        // Get the inputs used during this frame
        Inputs inputs = NoControlInputs;

        if (ControlsEnabled())
        {
            inputs = FetchInputs();
        }

        UpdateMovement(inputs);
        UpdatePossession(inputs);
    }

    protected virtual void OnUpdateNotPossessed()
    {
        if (Target && ControlsEnabled() && !IsPossessed && !HasDetectedTarget && (transform.position - Target.position).magnitude <= _distanceToDetect)
        {
            HasDetectedTarget = true;
        }
    }

    private IEnumerator UpdatePath()
    {
        Seeker.StartPath(transform.position, Target.position);

        yield return new WaitForSeconds(1.0f / UpdateRate);
        StartCoroutine(UpdatePath());
    }

    // Called when a new path is created
    public virtual void OnPathComplete(Path path)
    {
        // We got our path back
        if (path.error)
        {
            if (_logPathFailedError)
            {
                Debug.LogError("The path failed! " + gameObject.name);
            }

            return;
        }
    }

    protected bool IsTargetReachable()
    {
        return (Target.position - Path.vectorPath[Path.vectorPath.Count - 1]).magnitude < MinDistanceForTargetReachable;
    }

    protected abstract Inputs CreateInputs();

    protected override void UpdatePossession(Inputs inputs)
    {
        if (inputs.Possess && HasEnoughSpaceToUnpossess())
        {
            Unpossess();
        }
    }

    // Returns if the area to respawn the player is free. The area checked is based on the facing direction of the character
    private bool HasEnoughSpaceToUnpossess()
    {
        if ((SpriteRenderer.flipX && !_flipPlayerSpawn) || (!SpriteRenderer.flipX && _flipPlayerSpawn))
        {
            return _leftPlayerSpawn.OverlapCollider(_leftPlayerSpawnContactFilter, _overlapResults) == 0;
        }
        else
        {
            return _rightPlayerSpawn.OverlapCollider(_rightPlayerSpawnContactFilter, _overlapResults) == 0;
        }
    }

    public void SetTarget(Transform target)
    {
        Target = target;
    }

    // Returns the possession state after calling this method
    public bool Possess(Possession possessingScript)
    {
        if (IsPossessable && !IsPossessed)
        {
            _possessingScript = possessingScript;

            IsPossessed = true;

            _animator.SetLayerWeight(_possessedModeAnimationLayerIndex, 1.0f);
            VirtualCameraManager.Instance.ChangeVirtualCamera(_possessionVirtualCamera);

            AudioSource.pitch = Random.Range(.9f, 1.0f);
            AudioSource.PlayOneShot(_onPossessSound);

            OnPossess();
        }

        return IsPossessed;
    }

    protected virtual void OnPossess() { }

    // Returns the possession state after calling this method
    public bool Unpossess()
    {
        if (IsPossessed && IsPossessed)
        {
            if (_possessingScript)
            {
                // Select the correct player spawn and respawn facing direction
                Vector2 respawnPos;
                Vector2 respawnFacingDirection;

                if ((SpriteRenderer.flipX && !_flipPlayerSpawn) || (!SpriteRenderer.flipX && _flipPlayerSpawn))
                {
                    respawnPos = _leftPlayerSpawn.transform.position;
                    respawnFacingDirection = Vector2.left;
                }
                else
                {
                    respawnPos = _rightPlayerSpawn.transform.position;
                    respawnFacingDirection = Vector2.right;
                }
                
                // Tell the possession script, that took possession of this AIController, that isn't in control anymore
                _possessingScript.ReleasePossession(respawnPos, respawnFacingDirection);
            }

            IsPossessed = false;

            _animator.SetLayerWeight(_possessedModeAnimationLayerIndex, .0f);

            AudioSource.pitch = Random.Range(.9f, 1.0f);
            AudioSource.PlayOneShot(_onUnpossessSound);

            OnUnpossess();
        }

        return IsPossessed;
    }

    protected virtual void OnUnpossess() { }

    public void EnablePossession(bool enable)
    {
        IsPossessable = enable;
    }

    private void OnEnable()
    {
        Seeker.pathCallback += OnPathComplete;
    }

    private void OnDisable()
    {
        Seeker.pathCallback -= OnPathComplete;
    }

    private void OnDrawGizmosSelected()
    {
        if (_drawDistance)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, _distanceToDetect);
        }
    }
}
