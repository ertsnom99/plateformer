using Pathfinding;
using System.Collections;
using UnityEngine;

public abstract class AIControl : CharacterControl
{
    [Header("Target")]
    [SerializeField]
    protected Transform m_target;
    [SerializeField]
    private float m_distanceToDetect = 10.0f;
    [SerializeField]
    protected float m_stopDistanceToTarget = 1.4f;
    [SerializeField]
    protected float m_minDistanceForTargetReachable = 6.0f;

    public bool HasDetectedTarget { get; private set; }

    [Header("Update")]
    [SerializeField]
    protected int m_updateRate = 6;
    [SerializeField]
    protected float m_minDistanceToChangeWaypoint = 0.5f;
    [SerializeField]
    protected bool m_stopWhenUnreachable = true;
    
    [Header("Debug")]
    [SerializeField]
    private bool m_drawDistance = false;

    protected Path m_path;
    protected int m_targetWaypoint = 0;
    protected Seeker m_seeker;

    protected override void Awake()
    {
        base.Awake();
        
        m_seeker = GetComponent<Seeker>();
    }

    protected virtual void Start()
    {
        if (m_target == null)
        {
            Debug.LogError("No target was set!");
            return;
        }

        StartCoroutine(UpdatePath());
    }

    protected virtual void Update()
    {
        // Enable control when the target is close enough
        if (ControlsEnabled && !HasDetectedTarget && (transform.position - m_target.position).magnitude <= m_distanceToDetect)
        {
            EnableControl(true);
            HasDetectedTarget = true;
        }
    }

    private IEnumerator UpdatePath()
    {
        m_seeker.StartPath(transform.position, m_target.position);

        yield return new WaitForSeconds(1.0f / m_updateRate);
        StartCoroutine(UpdatePath());
    }

    // Called when a new path is created
    public abstract void OnPathComplete(Path path);

    protected bool IsTargetReachable()
    {
        return (m_target.position - m_path.vectorPath[m_path.vectorPath.Count - 1]).magnitude < m_minDistanceForTargetReachable;
    }

    protected abstract Inputs CreateInputs();
    
    public void SetTarget(Transform target)
    {
        m_target = target;
    }

    private void OnEnable()
    {
        m_seeker.pathCallback += OnPathComplete;
    }

    private void OnDisable()
    {
        m_seeker.pathCallback -= OnPathComplete;
    }

    private void OnDrawGizmosSelected()
    {
        if (m_drawDistance)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, m_distanceToDetect);
        }
    }
}
