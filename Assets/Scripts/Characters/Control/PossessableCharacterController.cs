using Cinemachine;
using Pathfinding;
using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public abstract class PossessableCharacterController : CharacterController, IPossessable
{
    [Header("Controls")]
    [SerializeField]
    protected bool UseKeyboard = false;

    protected Inputs NoControlInputs = new Inputs();

    [Header("Possession")]
    [SerializeField]
    private bool _isPossessable = true;

    public bool IsPossessable
    {
        get { return _isPossessable; }
        private set { _isPossessable = value; }
    }

    public bool IsPossessed { get; protected set; }

    [SerializeField]
    protected CinemachineVirtualCamera PossessionVirtualCamera;

    [SerializeField]
    protected AudioClip OnPossessSound;
    [SerializeField]
    protected AudioClip OnUnpossessSound;

    [SerializeField]
    protected Collider2D LeftPlayerSpawn;
    protected ContactFilter2D LeftPlayerSpawnContactFilter;
    [SerializeField]
    protected Collider2D RightPlayerSpawn;
    protected ContactFilter2D RightPlayerSpawnContactFilter;

    // Used to store result of overlap test
    protected Collider2D[] OverlapResults = new Collider2D[4];
    
    [SerializeField]
    protected bool FlipPlayerSpawn = false;

    protected Possession PossessingScript;

    /*[Header("Target")]
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
    protected int TargetWaypoint = 0;*/

    protected const string PossessedModeAnimationLayerName = "Possessed Mode";
    protected int PossessedModeAnimationLayerIndex;

    protected SpriteRenderer SpriteRenderer;
    protected Animator Animator;
    protected AudioSource AudioSource;
    //protected Seeker Seeker;

    protected virtual void Awake()
    {
        // Tells to use the layer settings from the Physics2D settings (the matrix)
        LeftPlayerSpawnContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(LeftPlayerSpawn.gameObject.layer));
        LeftPlayerSpawnContactFilter.useLayerMask = true;
        LeftPlayerSpawnContactFilter.useTriggers = false;

        RightPlayerSpawnContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(RightPlayerSpawn.gameObject.layer));
        RightPlayerSpawnContactFilter.useLayerMask = true;
        RightPlayerSpawnContactFilter.useTriggers = false;

        IsPossessed = false;
        //HasDetectedTarget = false;

        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();
        //Seeker = GetComponent<Seeker>();

        PossessedModeAnimationLayerIndex = Animator.GetLayerIndex(PossessedModeAnimationLayerName);
        Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, .0f);

        if (!LeftPlayerSpawn)
        {
            Debug.LogError("No left player spawn was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!RightPlayerSpawn)
        {
            Debug.LogError("No right player spawn was set for " + GetType() + " script of " + gameObject.name + "!");
        }
    }

    protected virtual void Start()
    {
        if (!VirtualCameraManager.Instance)
        {
            Debug.LogError("Couldn't find an instance of VirtualCameraManager for " + GetType() + " script of " + gameObject.name + "!");
        }
        
        if (!PossessionVirtualCamera)
        {
            Debug.LogError("No virtual camera was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        /*if (Target)
        {
            StartCoroutine(UpdatePath());
        }*/
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

        UpdatePossession(inputs);
    }

    protected abstract Inputs FetchInputs();

    protected virtual void UpdatePossession(Inputs inputs)
    {
        if (inputs.Possess && HasEnoughSpaceToUnpossess())
        {
            Unpossess();
        }
    }

    protected virtual void OnUpdateNotPossessed()
    {
        /*if (Target && ControlsEnabled() && !IsPossessed && !HasDetectedTarget && (transform.position - Target.position).magnitude <= _distanceToDetect)
        {
            HasDetectedTarget = true;
        }*/
    }

    /*private IEnumerator UpdatePath()
    {
        Seeker.StartPath(transform.position, Target.position);

        yield return new WaitForSeconds(1.0f / UpdateRate);
        StartCoroutine(UpdatePath());
    }*/

    // Called when a new path is created
    /*public virtual void OnPathComplete(Path path)
    {
        // We got our path back
        if (path.error)
        {
            if (_logPathFailedError)
            {
                Debug.LogError("The path failed for " + gameobject.name + "!");
            }

            return;
        }
    }*/

    /*protected bool IsTargetReachable()
    {
        return (Target.position - Path.vectorPath[Path.vectorPath.Count - 1]).magnitude < MinDistanceForTargetReachable;
    }*/

    protected abstract Inputs CreateInputs();

    // Returns if the area to respawn the player is free. The area checked is based on the facing direction of the character
    protected virtual bool HasEnoughSpaceToUnpossess()
    {
        if ((SpriteRenderer.flipX && !FlipPlayerSpawn) || (!SpriteRenderer.flipX && FlipPlayerSpawn))
        {
            return LeftPlayerSpawn.OverlapCollider(LeftPlayerSpawnContactFilter, OverlapResults) == 0;
        }
        else
        {
            return RightPlayerSpawn.OverlapCollider(RightPlayerSpawnContactFilter, OverlapResults) == 0;
        }
    }

    /*public void SetTarget(Transform target)
    {
        Target = target;
    }*/
    
    public void EnablePossession(bool enable)
    {
        IsPossessable = enable;
    }

    /*private void OnEnable()
    {
        Seeker.pathCallback += OnPathComplete;
    }*/

    /*private void OnDisable()
    {
        Seeker.pathCallback -= OnPathComplete;
    }*/

    /*private void OnDrawGizmosSelected()
    {
        if (_drawDistance)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, _distanceToDetect);
        }
    }*/

    public void SetKeyboardUse(bool useKeyboard)
    {
        UseKeyboard = useKeyboard;
    }

    // Methods of the IPossessable interface
    public virtual bool Possess(Possession possessingScript)
    {
        if (IsPossessable && !IsPossessed)
        {
            PossessingScript = possessingScript;

            IsPossessed = true;

            Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, 1.0f);
            VirtualCameraManager.Instance.ChangeVirtualCamera(PossessionVirtualCamera);

            AudioSource.pitch = Random.Range(.9f, 1.0f);
            AudioSource.PlayOneShot(OnPossessSound);

            OnPossess(possessingScript);
        }

        return IsPossessed;
    }

    protected virtual void OnPossess(Possession possessingScript) { }

    public virtual bool Unpossess(bool centerColliderToPos = false, Vector2? forceRespawnPos = null)
    {
        if (IsPossessed && IsPossessed)
        {
            if (PossessingScript)
            {
                // Select the correct player spawn and respawn facing direction
                Vector2 respawnPos;
                Vector2 respawnFacingDirection;

                if ((SpriteRenderer.flipX && !FlipPlayerSpawn) || (!SpriteRenderer.flipX && FlipPlayerSpawn))
                {
                    respawnPos = LeftPlayerSpawn.transform.position;
                    respawnFacingDirection = Vector2.left;
                }
                else
                {
                    respawnPos = RightPlayerSpawn.transform.position;
                    respawnFacingDirection = Vector2.right;
                }

                if (forceRespawnPos != null)
                {
                    respawnPos = (Vector2)forceRespawnPos;
                }

                // Tell the possession script, that took possession of this AIController, that isn't in control anymore
                PossessingScript.ReleasePossession(respawnPos, respawnFacingDirection, centerColliderToPos);
            }

            IsPossessed = false;

            Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, .0f);

            AudioSource.pitch = Random.Range(.9f, 1.0f);
            AudioSource.PlayOneShot(OnUnpossessSound);

            OnUnpossess();
        }

        return IsPossessed;
    }

    protected virtual void OnUnpossess() { }
}
