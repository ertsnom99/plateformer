using Pathfinding;
using System.Collections;
using UnityEngine;

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
    protected Seeker Seeker;

    protected virtual void Awake()
    {
        IsPossessed = false;
        HasDetectedTarget = false;

        Seeker = GetComponent<Seeker>();
    }

    protected virtual void Start()
    {
        if (Target == null)
        {
            Debug.LogError("No target was set!");
            return;
        }
        
        StartCoroutine(UpdatePath());
    }

    protected virtual void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > .0f && ControlsEnabled())
        {
            if (IsPossessed)
            {
                // Get the inputs used during this frame
                Inputs inputs = FetchInputs();

                UpdateMovement(inputs);

                OnUpdatePossessed();
            }
            else
            {
                if (!IsPossessed && !HasDetectedTarget && (transform.position - Target.position).magnitude <= _distanceToDetect)
                {
                    HasDetectedTarget = true;
                }
                
                OnUpdateNotPossessed();
            }
        }
    }

    protected virtual void OnUpdatePossessed() { }

    protected virtual void OnUpdateNotPossessed() { }

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
    
    public void SetTarget(Transform target)
    {
        Target = target;
    }

    // Returns the possession state after calling this method
    public bool Possess(bool possess)
    {
        if ((IsPossessable || IsPossessed) && IsPossessed != possess)
        {
            IsPossessed = possess;
            OnPossessionChange();
        }

        return IsPossessed;
    }

    protected virtual void OnPossessionChange() { }

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
