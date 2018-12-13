using UnityEngine;
using Pathfinding;
using System.Collections;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Seeker))]

public class FlyingAIControl : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform m_target;
    [SerializeField]
    private float m_stopDistanceToTarget = 1.4f;
    [SerializeField]
    private float m_minDistanceForTargetReachable = 6.0f;

    [Header("Update")]
    [SerializeField]
    private int m_updateRate = 2;
    [SerializeField]
    private float m_nextWaypointDistance = 0.1f;
    [SerializeField]
    private bool m_stopWhenUnreachable = true;

    private Path m_path;
    private int m_targetWaypoint = 0;

    private Inputs noControlInputs;

    public bool ControlsEnabled { get; private set; }

    private FlyingMovement m_movementScript;
    private Seeker m_seeker;

    private void Awake()
    {
        noControlInputs = new Inputs();
        ControlsEnabled = true;

        m_movementScript = GetComponent<FlyingMovement>();
        m_seeker = GetComponent<Seeker>();
    }

    private void Start()
    {
        if (m_target == null)
        {
            Debug.LogError("No target was set!");
            return;
        }

        StartCoroutine(UpdatePath());
    }

    private IEnumerator UpdatePath()
    {
        m_seeker.StartPath(transform.position, m_target.position);

        yield return new WaitForSeconds(1.0f / m_updateRate);
        StartCoroutine(UpdatePath());
    }

    public void OnPathComplete(Path path)
    {
        // We got our path back
        if (path.error)
        {
            Debug.LogError("The path failed!");
        }
        else
        {
            m_path = path;
            m_targetWaypoint = 0;
        }
    }
    
    private void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > .0f)
        {
            Inputs inputs = noControlInputs;

            if (ControlsCharacter())
            {
                if (m_path != null)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, m_target.position);
                    bool isWaypointReached = m_targetWaypoint >= m_path.vectorPath.Count;

                    // Check if the AI hasn't reach either the target or the last waypoint
                    if ((!m_stopWhenUnreachable || IsTargetReachable()) && distanceToTarget > m_stopDistanceToTarget && !isWaypointReached)
                    {
                        float distanceToWaypoint = Vector3.Distance(transform.position, m_path.vectorPath[m_targetWaypoint]);

                        // Update the target waypoint while the last one hasn't been reached and the current one is close enough
                        while (!isWaypointReached && distanceToWaypoint <= m_nextWaypointDistance)
                        {
                            m_targetWaypoint++;

                            isWaypointReached = m_targetWaypoint >= m_path.vectorPath.Count;

                            if (!isWaypointReached)
                            {
                                distanceToWaypoint = Vector3.Distance(transform.position, m_path.vectorPath[m_targetWaypoint]);
                            }

                        }

                        // Create the inputs if it wasn't done in the previous
                        if (!isWaypointReached)
                        {
                            inputs = CreateInputs();
                        }
                    }
                }
            }

            UpdateMovement(inputs);
        }
    }

    private bool IsTargetReachable()
    {
        return (m_target.position - m_path.vectorPath[m_path.vectorPath.Count - 1]).magnitude < m_minDistanceForTargetReachable;
    }

    private Inputs CreateInputs()
    {
        Inputs inputs = noControlInputs;

        Vector3 DirectionToTargetWaypoint = (m_path.vectorPath[m_targetWaypoint] - transform.position).normalized;

        // Inputs from the controler
        inputs.vertical = DirectionToTargetWaypoint.y;
        inputs.horizontal = DirectionToTargetWaypoint.x;
        //inputs.jump = jump;
        //inputs.releaseJump = releaseJump;
        //inputs.dash = Input.GetButtonDown("Dash");
        //inputs.releaseDash = Input.GetButtonUp("Dash");

        return inputs;
    }

    private bool ControlsCharacter()
    {
        return ControlsEnabled;
    }

    private void UpdateMovement(Inputs inputs)
    {
        m_movementScript.SetInputs(inputs);
    }

    private void OnEnable()
    {
        m_seeker.pathCallback += OnPathComplete;
    }
    private void OnDisable()
    {
        m_seeker.pathCallback -= OnPathComplete;
    }
}
