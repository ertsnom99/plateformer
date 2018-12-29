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
    [SerializeField]
    private float m_pathFixByAngleThreshold = .1f;

    [Header("Update")]
    [SerializeField]
    private int m_updateRate = 2;
    [SerializeField]
    private float m_minDistanceToChangeWaypoint = 0.2f;
    [SerializeField]
    private bool m_stopWhenUnreachable = true;
    private Vector2 m_previousToCurrentWaypoint;
    private float m_horizontalInputForJump = .0f;

    [Header("Vertical Movement")]
    [SerializeField]
    private bool m_canJump = true;
    [SerializeField]
    private float m_minHeightToJump = 0.4f;
    [SerializeField]
    private float m_JumpHorizontalDistance = 1.0f;
    [SerializeField]
    private float m_DropDownHorizontalDistance = 1.2f;
    [SerializeField]
    private float m_minHeightToReleaseJump = 0.1f;
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
            
            // Fix the path has long has it needs to be
            while (m_path.vectorPath.Count >= 3 && PathNeedsFix())
            {
                m_path.vectorPath.RemoveAt(1);
                Debug.Log(Time.frameCount + " Fixed");
            }

            // Normally, the first point of the path SHOULD be extremely close to the position of the AI,
            // therefore it already reached the first waypoint which mean we can skip waypoint 0
            m_targetWaypoint = 1;
            m_previousToCurrentWaypoint = m_path.vectorPath[m_targetWaypoint] - m_path.vectorPath[m_targetWaypoint - 1];

            // If a vertical movement is needed to reach the current waypoint, the horizontal input can be calculated immediately
            if (m_previousToCurrentWaypoint.y >= m_minHeightToJump || m_previousToCurrentWaypoint.y <= -m_minHeightToJump)
            {
                CalculateHorizontalInputForVerticalMovement();
            }
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

            if (ControlsCharacter() && m_path != null)
            {
                bool isWaypointReached = m_targetWaypoint >= m_path.vectorPath.Count;
                float distanceToTarget = Vector3.Distance(transform.position, m_target.position);
                    
                // Check if the AI hasn't reach either the target or the last waypoint
                if ((!m_stopWhenUnreachable || IsTargetReachable()) && !isWaypointReached && distanceToTarget > m_stopDistanceToTarget)
                {
                    Vector2 positionToTargetWaypoint = m_path.vectorPath[m_targetWaypoint] - transform.position;

                    // Update the target waypoint while the last one hasn't been reached and the current one has been past
                    while (!isWaypointReached && (Vector2.Angle(m_previousToCurrentWaypoint, positionToTargetWaypoint) >= 90.0f || positionToTargetWaypoint.magnitude <= m_minDistanceToChangeWaypoint))
                    {
                        m_targetWaypoint++;

                        isWaypointReached = m_targetWaypoint >= m_path.vectorPath.Count;

                        if (!isWaypointReached)
                        {
                            m_previousToCurrentWaypoint = m_path.vectorPath[m_targetWaypoint] - m_path.vectorPath[m_targetWaypoint - 1];
                            positionToTargetWaypoint = m_path.vectorPath[m_targetWaypoint] - transform.position;

                            // If a jump is needed to reach the current waypoint,
                            // the horizontal input can be calculated immediatly
                            if (m_previousToCurrentWaypoint.y >= m_minHeightToJump || m_previousToCurrentWaypoint.y <= -m_minHeightToJump)
                            {
                                CalculateHorizontalInputForVerticalMovement();
                            }
                        }
                    }

                    // Create the inputs if it wasn't done in the previous
                    if (!isWaypointReached)
                    {
                        inputs = CreateInputs();
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

    private void CalculateHorizontalInputForVerticalMovement()
    {
        // Calculate how much time will be needed to complete the horizontal movement
        // Based on h=vi*t+(g*t^2)/2 formula, but solved to isolate t
        float initialJumpVelocity;
        float gravityUsed;
        float horizontalMovementModifier;

        // Use different values based on vertical movement direction
        if (m_previousToCurrentWaypoint.y >= .0f)
        {
            initialJumpVelocity = m_movementScript.JumpTakeOffSpeed;
            gravityUsed = m_movementScript.CurrentGravityModifier * Physics2D.gravity.y;
            horizontalMovementModifier = m_JumpHorizontalDistance;
        }
        else
        {
            initialJumpVelocity = .0f;
            gravityUsed = Mathf.Abs(m_movementScript.CurrentGravityModifier * Physics2D.gravity.y);
            horizontalMovementModifier = m_DropDownHorizontalDistance;
        }

        float verticalMovement = Mathf.Abs(m_previousToCurrentWaypoint.y);
        float predictedTimeToReach = (Mathf.Sqrt(Mathf.Pow(initialJumpVelocity, 2) + 2.0f * gravityUsed * verticalMovement) - initialJumpVelocity) / gravityUsed;
        
        // Calculate how much distance/sec is need to cover the horizontal part of the movement
        float distanceBySecond = (m_previousToCurrentWaypoint.x * horizontalMovementModifier) / predictedTimeToReach;

        // Calculate how much of the horizontal speed is necessary and save it for later use
        m_horizontalInputForJump = Mathf.Clamp(distanceBySecond / m_movementScript.MaxSpeed, -1.0f, 1.0f);
    }

    private Inputs CreateInputs()
    {
        Inputs inputs = noControlInputs;

        Vector3 positionToTargetWaypoint = m_path.vectorPath[m_targetWaypoint] - transform.position;

        // Check jump inputs
        bool jumpNeededToReachNextWaypoint = m_previousToCurrentWaypoint.y >= m_minHeightToJump;
        bool jump = m_canJump && !m_jumpInputDown && jumpNeededToReachNextWaypoint && m_movementScript.IsGrounded;
        bool releaseJump = m_jumpInputDown && (positionToTargetWaypoint.y <= -m_minHeightToReleaseJump || m_movementScript.Velocity.y < .0f);

        if (jump)
        {
            m_jumpInputDown = true;
        }
        else if (releaseJump)
        {
            m_jumpInputDown = false;
        }

        // The horizontal input must be calculate differently during a horizontal movement (before releasing the jump input in the case of a jump)
        float horizontalInput;

        if ((jumpNeededToReachNextWaypoint && m_jumpInputDown) || m_previousToCurrentWaypoint.y <= -m_minHeightToJump)
        {
            horizontalInput = m_horizontalInputForJump;
        }
        else
        {
            horizontalInput = Mathf.Sign(positionToTargetWaypoint.x);
        }

        // Inputs from the controler
        //inputs.vertical = Input.GetAxisRaw("Vertical");
        inputs.horizontal = horizontalInput;
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
