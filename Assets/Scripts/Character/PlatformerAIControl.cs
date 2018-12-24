using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

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
    [SerializeField]
    private float m_pathFixByAngleThreshold = .1f;

    [Header("Update")]
    [SerializeField]
    private int m_updateRate = 2;
    [SerializeField]
    private bool m_stopWhenUnreachable = true;
    private Vector2 m_previousToCurrentWaypoint;

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

    // Called when a new path is created
    public void OnPathComplete(Path path)
    {
        if (path.error)
        {
            Debug.LogError("The path failed!");
        }
        else
        {
            m_path = path;

            while (m_path.vectorPath.Count >= 3 && PathNeedsFix())
            {
                m_path.vectorPath.RemoveAt(1);
                Debug.Log(Time.frameCount + " Fixed");
            }

            m_targetWaypoint = 1;
            m_previousToCurrentWaypoint = m_path.vectorPath[m_targetWaypoint] - m_path.vectorPath[m_targetWaypoint - 1];
        }
    }

    private bool PathNeedsFix()
    {
        Vector2 secondToFirstWaypoint = m_path.vectorPath[0] - m_path.vectorPath[1];
        Vector2 secondToThirdWaypoint = m_path.vectorPath[2] - m_path.vectorPath[1];

        // Check if both vectors are along the exact same line and in the same direction
        return Vector2.Angle(secondToFirstWaypoint, secondToThirdWaypoint) <= m_pathFixByAngleThreshold;
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
                    bool isWaypointReached = m_targetWaypoint >= m_path.vectorPath.Count;
                    float distanceToTarget = Vector3.Distance(transform.position, m_target.position);
                    
                    // Check if the AI hasn't reach either the target or the last waypoint
                    if ((!m_stopWhenUnreachable || IsTargetReachable()) && !isWaypointReached && distanceToTarget > m_stopDistanceToTarget)
                    {
                        Vector2 positionToTargetWaypoint = m_path.vectorPath[m_targetWaypoint] - transform.position;

                        // Update the target waypoint while the last one hasn't been reached and the current one has been past
                        while (!isWaypointReached && Vector2.Angle(m_previousToCurrentWaypoint, positionToTargetWaypoint) >= 90.0f)
                        {
                            m_targetWaypoint++;

                            isWaypointReached = m_targetWaypoint >= m_path.vectorPath.Count;

                            if (!isWaypointReached)
                            {
                                m_previousToCurrentWaypoint = m_path.vectorPath[m_targetWaypoint] - m_path.vectorPath[m_targetWaypoint - 1];
                                positionToTargetWaypoint = m_path.vectorPath[m_targetWaypoint] - transform.position;
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

        // TODO: Rework horizontal movement
        // HACK: Normally, if the path was correct, it wouldn't tell to jump anyway. This is a quick fixe since the path generation has issues
        // Choose horizontal movement
        float horizontalMovement = m_canJump || !jumpNeededToReachNextWaypoint ? Mathf.Sign(positionToTargetWaypoint.x) : .0f ;

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
        inputs.jump = false;//jump;
        inputs.releaseJump = false;//releaseJump;
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
