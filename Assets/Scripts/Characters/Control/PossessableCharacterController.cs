using Cinemachine;
using Pathfinding;
using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Seeker))]

public abstract class PossessableCharacterController : CharacterController, IPossessable
{
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
    protected GameObject InfoUI;

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

    protected PossessionPower PossessingScript;
    protected PlayerController PossessingController;

    [Header("AI Target")]
    [SerializeField]
    protected Transform Target;
    [SerializeField]
    private float _distanceToDetect = 10.0f;
    [SerializeField]
    protected float StopDistanceToTarget = .4f;
    [SerializeField]
    protected float MinDistanceForTargetReachable = 4.0f;

    public bool HasDetectedTarget { get; private set; }

    [Header("AI Update")]
    [SerializeField]
    protected int UpdateRate = 5;
    [SerializeField]
    protected float MinDistanceToChangeWaypoint = 0.5f;
    [SerializeField]
    protected bool StopWhenUnreachable = true;

    [Header("Debug")]
    [SerializeField]
    private bool _drawDistanceToDetect = false;
    [SerializeField]
    private bool _logPathFailedError = false;

    protected Path Path;
    protected int TargetWaypoint = 0;

    protected const string PossessedModeAnimationLayerName = "Possessed Mode";
    protected int PossessedModeAnimationLayerIndex;

    protected SpriteRenderer SpriteRenderer;
    protected Animator Animator;
    protected AudioSource AudioSource;
    protected Seeker Seeker;

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
        HasDetectedTarget = false;

        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();
        Seeker = GetComponent<Seeker>();

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
    
    protected abstract void OnUpdatePossessed();

    protected virtual void UpdateDisplayInfo(Inputs inputs)
    {
        if (inputs.PressHelp)
        {
            if (InfoUI)
            {
                InfoUI.SetActive(!InfoUI.activeSelf);
            }
            else
            {
                Debug.LogError("No info UI was set for " + GetType() + " script of " + gameObject.name + "!");
            }
        }
    }

    protected virtual void UpdatePossession(Inputs inputs)
    {
        if (inputs.PressPossess && HasEnoughSpaceToUnpossess())
        {
            Unpossess();
        }
    }

    // Returns if the area to respawn the player is free. The area checked is based on the facing direction of the character
    protected virtual bool HasEnoughSpaceToUnpossess()
    {
        if (UseLeftSpawn())
        {
            return LeftPlayerSpawn.OverlapCollider(LeftPlayerSpawnContactFilter, OverlapResults) == 0;
        }
        else
        {
            return RightPlayerSpawn.OverlapCollider(RightPlayerSpawnContactFilter, OverlapResults) == 0;
        }
    }

    // Used to determine if the left spawn must be use when ever a choice needs to be made between the left and right spawn
    protected virtual bool UseLeftSpawn()
    {
        return SpriteRenderer.flipX;
    }

    protected virtual void OnUpdateNotPossessed()
    {
        if (Target && ControlsEnabled() && !IsPossessed && !HasDetectedTarget && (transform.position - Target.position).magnitude <= _distanceToDetect)
        {
            HasDetectedTarget = true;
            StartCoroutine(UpdatePath());
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
                Debug.LogError("The path failed for " + gameObject.name + "!");
            }

            return;
        }

        Path = path;
        TargetWaypoint = 0;
    }

    protected bool IsTargetReachable()
    {
        return Target != null &&
               Path != null &&
               (Target.position - Path.vectorPath[Path.vectorPath.Count - 1]).magnitude < MinDistanceForTargetReachable;
    }

    protected virtual void OnPossess(PossessionPower possessingScript) { }
    
    protected void TransferPossession(PossessableCharacterController controller)
    {
        Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, .0f);
        IsPossessed = false;

        PossessingScript.TakePossession(controller);
    }

    protected virtual void OnUnpossess() { }

    protected abstract Inputs CreateInputs();

    public void EnablePossession(bool enable)
    {
        IsPossessable = enable;
    }

    public void SetInfoUI(GameObject infoUI)
    {
        InfoUI = infoUI;
    }

    public void SetPossessionVirtualCamera(CinemachineVirtualCamera possessionVirtualCamera)
    {
        PossessionVirtualCamera = possessionVirtualCamera;
    }

    public void SetTarget(Transform target)
    {
        if (target != Target)
        {
            StopAllCoroutines();
            HasDetectedTarget = false;
            Path = null;

            Target = target;
        }
    }
    
    public void SetDistanceToDetect(float distanceToDetect)
    {
        _distanceToDetect = distanceToDetect;
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
        if (_drawDistanceToDetect)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, _distanceToDetect);
        }
    }

    // Methods of the IPossessable interface
    public virtual bool Possess(PossessionPower possessingScript, PlayerController possessingController)
    {
        if (IsPossessable && !IsPossessed)
        {
            PossessingScript = possessingScript;
            PossessingController = possessingController;

            IsPossessed = true;

            Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, 1.0f);

            VirtualCameraManager.Instance.ChangeVirtualCamera(PossessionVirtualCamera);

            AudioSource.pitch = Random.Range(.9f, 1.0f);
            AudioSource.PlayOneShot(OnPossessSound);

            OnPossess(possessingScript);
        }

        return IsPossessed;
    }

    public virtual GameObject Unpossess(bool centerColliderToPos = false, Vector2? forceRespawnPos = null)
    {
        GameObject spawnedCharacter = null;

        if (IsPossessable && IsPossessed)
        {
            if (PossessingScript)
            {
                // Select the correct player spawn and respawn facing direction
                Vector2 respawnPos;
                Vector2 respawnFacingDirection;

                if (UseLeftSpawn())
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
                
                spawnedCharacter = PossessingScript.gameObject;

                PossessingController = null;
                PossessingScript = null;
            }

            InfoUI.SetActive(false);

            IsPossessed = false;

            Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, .0f);

            AudioSource.pitch = Random.Range(.9f, 1.0f);
            AudioSource.PlayOneShot(OnUnpossessSound);

            OnUnpossess();
        }

        // HACK: Not sure if this method should just return if "IsPossessed"
        return spawnedCharacter;
    }
}
