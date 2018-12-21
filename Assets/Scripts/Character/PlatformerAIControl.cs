using UnityEngine;
using Pathfinding;
using System.Collections;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(Seeker))]

public class PlatformerAIControl : MonoBehaviour
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

    [Header("Jump")]
    [SerializeField]
    private bool m_canJump = true;
    [SerializeField]
    private float m_minHeightToJump = 0.4f;
    [SerializeField]
    private float m_minHeightToReleaseJump = 1.5f;
    private bool m_jumpInputDown = false;

    private Path m_path;
    private int m_targetWaypoint = 0;

    private Inputs noControlInputs;

    public bool ControlsEnabled { get; private set; }

    private PlatformerMovement m_movementScript;
    private Seeker m_seeker;

    private void Awake()
    {
        noControlInputs = new Inputs();
        ControlsEnabled = true;

        m_movementScript = GetComponent<PlatformerMovement>();
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

    private bool PathNeedsFix()
    {
        return (m_path.vectorPath[0].x > m_path.vectorPath[1].x && m_path.vectorPath[1].x < m_path.vectorPath[2].x)
            || (m_path.vectorPath[0].x < m_path.vectorPath[1].x && m_path.vectorPath[1].x > m_path.vectorPath[2].x);
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
                    /*for(int i = 0; i < m_path.vectorPath.Count; i++)
                    {
                        Debug.Log(m_path.vectorPath[i].x + " : " + m_path.vectorPath[i].y);
                    }
                    Debug.Log("-----------------------------------------------------");*/

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
        
        Vector3 positionToTargetWaypoint = m_path.vectorPath[m_targetWaypoint] - transform.position;
        bool jumpNeededToReachNextWaypoint = positionToTargetWaypoint.y >= m_minHeightToJump || positionToTargetWaypoint.y <= -m_minHeightToJump;

        // HACK: Normally, if the path was correct, it wouldn't tell to jump anyway. This is a quick fixe since the path generation has issues
        // Choose horizontal movement
        float horizontalMovement = m_canJump || !jumpNeededToReachNextWaypoint ? positionToTargetWaypoint.normalized.x : .0f ;

        // Check jump inputs
        bool jump = m_canJump && !m_jumpInputDown && jumpNeededToReachNextWaypoint && m_movementScript.IsGrounded;
        
        if (jump && !m_jumpInputDown)
        {
            m_jumpInputDown = true;
        }
        
        bool releaseJump = m_jumpInputDown && positionToTargetWaypoint.y <= -m_minHeightToReleaseJump;

        if (m_jumpInputDown && releaseJump)
        {
            m_jumpInputDown = false;
        }

        /*if (positionToTargetWaypoint.normalized.x < .0f && (m_target.transform.position.y - transform.position.y) <= 2.0f)
        {
            Debug.Log(Time.time + "---    " + positionToTargetWaypoint.normalized.x + " : " + positionToTargetWaypoint.normalized.y);
            Debug.Log(Time.time + "---" + transform.position.x + " : " + transform.position.y);
            Time.timeScale = .0f;
            Debug.Log(Time.time + " -----------Freeze-----------------------------------------------------");
            for (int i = 0; i < m_path.vectorPath.Count; i++)
            {
                Debug.Log(Time.time + "---" + m_path.vectorPath[i].x + " : " + m_path.vectorPath[i].y);
            }
            Debug.Log(Time.time + "-----------------------------------------------------");
        }*/

        // Inputs from the controler
        //inputs.vertical = Input.GetAxisRaw("Vertical");
        inputs.horizontal = horizontalMovement;
        inputs.jump = jump;
        inputs.releaseJump = releaseJump;
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
