using Pathfinding;
using System.Collections;
using UnityEngine;

public abstract class AIControl : CharacterControl
{
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

    protected Path Path;
    protected int TargetWaypoint = 0;
    protected Seeker Seeker;

    protected override void Awake()
    {
        base.Awake();
        
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
        // Enable control when the target is close enough
        if (ControlsEnabled && !HasDetectedTarget && (transform.position - Target.position).magnitude <= _distanceToDetect)
        {
            EnableControl(true);
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
    public abstract void OnPathComplete(Path path);

    protected bool IsTargetReachable()
    {
        return (Target.position - Path.vectorPath[Path.vectorPath.Count - 1]).magnitude < MinDistanceForTargetReachable;
    }

    protected abstract Inputs CreateInputs();
    
    public void SetTarget(Transform target)
    {
        Target = target;
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
