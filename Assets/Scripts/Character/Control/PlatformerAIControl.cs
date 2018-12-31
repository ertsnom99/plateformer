using UnityEngine;
using Pathfinding;
using System.Collections;

public enum PathLinkType
{
    Walk = 0,
    Jump,
    DropDown
}

public struct PathLink
{
    public Vector2 start;
    public Vector2 end;
    public Vector2 link;
    public PathLinkType type;

    public PathLink(Vector2 pStart, Vector2 pEnd, float minHeightForVerticalMovement)
    {
        start = pStart;
        end = pEnd;
        link = end - start;

        if (link.y >= minHeightForVerticalMovement)
        {
            type = PathLinkType.Jump;
        }
        else if (link.y <= -minHeightForVerticalMovement)
        {
            type = PathLinkType.DropDown;
        }
        else
        {
            type = PathLinkType.Walk;
        }
    }
}

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
    private int m_updateRate = 6;
    [SerializeField]
    private float m_minDistanceToChangeWaypoint = 0.2f;
    [SerializeField]
    private bool m_stopWhenUnreachable = true;

    private PathLink m_currentPathLink;
    private bool m_verticalMovementinProgress = false;
    private bool m_delayedMovementProgressCheck = false;
    private float m_horizontalInputForVerticalMovement = .0f;

    [Header("Vertical Movement")]
    [SerializeField]
    private bool m_canJump = true;
    [SerializeField]
    private float m_minHeightForVerticalMovement = 0.4f;
    [SerializeField]
    private float m_minDropDownWidthToJump = 2.0f;
    [SerializeField]
    private float m_minHeightToReleaseJump = 0.1f;
    private bool m_jumpInputDown = false;

    private Path m_path;
    private int m_targetWaypoint = 0;

    private Inputs noControlInputs;

    public bool ControlsEnabled { get; private set; }

    private PlatformerMovement m_movementScript;
    private Seeker m_seeker;

    private const float JumpHorizontalMovementModifier = 1.0f;
    private const float DropDownHorizontalMovementModifier = 1.0f;
    private const float PathFixByAngleThreshold = .1f;
    private const float MinDistanceToMoveDuringVerticalMovement = 0.4f;

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

            // Create the path link
            PathLink previousPathLink = m_currentPathLink;
            m_currentPathLink = new PathLink(m_path.vectorPath[m_targetWaypoint - 1], m_path.vectorPath[m_targetWaypoint], m_minHeightForVerticalMovement);

            // If the vertical movement didn't start and it is needed to reach the current waypoint
            if (!m_verticalMovementinProgress && m_currentPathLink.type != PathLinkType.Walk)
            {
                m_horizontalInputForVerticalMovement = CalculateHorizontalInputForVerticalMovement();
                m_verticalMovementinProgress = true;
                m_delayedMovementProgressCheck = true;
            }
            else if (!m_verticalMovementinProgress && m_currentPathLink.type == PathLinkType.Walk)
            {
                m_verticalMovementinProgress = false;
                m_delayedMovementProgressCheck = false;
            }
            // If the vertical movement did start, check if the PathLinkType might need to be adjust
            // For exemple:
            // link got shorter and doens't have a big enough y to be created has a jump link
            // was a jump link, but suddenly became a drop down link
            else if (m_verticalMovementinProgress && m_currentPathLink.link.y > .0f)
            {
                if (m_currentPathLink.type != PathLinkType.Jump)
                {
                    m_currentPathLink.type = PathLinkType.Jump;
                }

                // Check if the PathLinkType pass from a drop down to a jump
                if (previousPathLink.type == PathLinkType.DropDown)
                {
                    // TODO: Addapt calculation
                    m_horizontalInputForVerticalMovement = CalculateHorizontalInputForVerticalMovement();
                    m_delayedMovementProgressCheck = true;
                }
            }
            else if (m_verticalMovementinProgress && m_currentPathLink.link.y < .0f)
            {
                if (m_currentPathLink.type != PathLinkType.DropDown)
                {
                    m_currentPathLink.type = PathLinkType.DropDown;
                }

                // Check if the PathLinkType pass from a jump to a drop down
                if (previousPathLink.type == PathLinkType.Jump)
                {
                    m_horizontalInputForVerticalMovement = CalculateHorizontalInputForVerticalMovement();
                    m_delayedMovementProgressCheck = true;
                }
            }
        }
    }

    private bool PathNeedsFix()
    {
        Vector2 secondToFirstWaypoint = m_path.vectorPath[0] - m_path.vectorPath[1];
        Vector2 secondToThirdWaypoint = m_path.vectorPath[2] - m_path.vectorPath[1];

        // Check if both vectors are along the exact same line and in the same direction
        return Vector2.Angle(secondToFirstWaypoint, secondToThirdWaypoint) <= PathFixByAngleThreshold;
    }

    private void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > .0f)
        {
            // Wait for the delay to end, before checking if the vertical movement is in progress 
            if (m_delayedMovementProgressCheck && !m_movementScript.IsGrounded && ((m_currentPathLink.link.y < .0f && m_movementScript.Velocity.y < .0f) || (m_currentPathLink.link.y > .0f && m_movementScript.Velocity.y > .0f)))
            {
                m_delayedMovementProgressCheck = false;
            }
            else if (!m_delayedMovementProgressCheck && m_verticalMovementinProgress && m_movementScript.IsGrounded)
            {
                m_verticalMovementinProgress = false;
            }

            // The horizontal velocity might need to be adjusted
            if (m_movementScript.IsGrounded && Mathf.Sign(m_horizontalInputForVerticalMovement) != Mathf.Sign(m_currentPathLink.link.x))
            {
                m_horizontalInputForVerticalMovement = CalculateHorizontalInputForVerticalMovement();
            }

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
                    while (!isWaypointReached && (IsJumpAndDropDownLinkOver() || IsWalkLinkOver(positionToTargetWaypoint)))
                    {
                        m_targetWaypoint++;

                        isWaypointReached = m_targetWaypoint >= m_path.vectorPath.Count;

                        if (!isWaypointReached)
                        {
                            m_currentPathLink = new PathLink(m_path.vectorPath[m_targetWaypoint - 1], m_path.vectorPath[m_targetWaypoint], m_minHeightForVerticalMovement);
                            positionToTargetWaypoint = m_path.vectorPath[m_targetWaypoint] - transform.position;

                            // If a vertical movement is needed to reach the current waypoint, the horizontal input can be calculated immediatly
                            if (m_currentPathLink.type != PathLinkType.Walk)
                            {
                                m_horizontalInputForVerticalMovement = CalculateHorizontalInputForVerticalMovement();
                                m_verticalMovementinProgress = true;
                                m_delayedMovementProgressCheck = true;
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

    private bool IsWalkLinkOver(Vector2 positionToTargetWaypoint)
    {
        return m_currentPathLink.type == PathLinkType.Walk && (Vector2.Angle(m_currentPathLink.link, positionToTargetWaypoint) >= 90.0f || positionToTargetWaypoint.magnitude <= m_minDistanceToChangeWaypoint);
    }

    private bool IsJumpAndDropDownLinkOver()
    {
        return (m_currentPathLink.type == PathLinkType.DropDown || m_currentPathLink.type == PathLinkType.Jump) && !m_verticalMovementinProgress;
    }

    private float CalculateHorizontalInputForVerticalMovement()
    {
        // Calculate how much time will be needed to complete the horizontal movement
        // Based on h=vi*t+(g*t^2)/2 formula, but solved to isolate t
        float initialVelocity;
        float gravityUsed;
        float horizontalMovementModifier;

        // Use different values based on vertical movement direction
        if (m_currentPathLink.link.y >= .0f)
        {
            initialVelocity = m_movementScript.JumpTakeOffSpeed;
            gravityUsed = m_movementScript.CurrentGravityModifier * Physics2D.gravity.y;
            horizontalMovementModifier = JumpHorizontalMovementModifier;
        }
        else
        {
            initialVelocity = .0f;
            gravityUsed = Mathf.Abs(m_movementScript.CurrentGravityModifier * Physics2D.gravity.y);
            horizontalMovementModifier = DropDownHorizontalMovementModifier;
        }

        float verticalMovement = Mathf.Abs(m_currentPathLink.link.y);
        float predictedTimeToReach = (Mathf.Sqrt(Mathf.Pow(initialVelocity, 2) + 2.0f * gravityUsed * verticalMovement) - initialVelocity) / gravityUsed;
        
        // Calculate how much distance/sec is need to cover the horizontal part of the movement
        float distanceBySecond = (m_currentPathLink.link.x * horizontalMovementModifier) / predictedTimeToReach;

        // Calculate how much of the horizontal speed is necessary and save it for later use
        return Mathf.Clamp(distanceBySecond / m_movementScript.MaxSpeed, -1.0f, 1.0f);
    }

    private Inputs CreateInputs()
    {
        Inputs inputs = noControlInputs;

        Vector3 positionToTargetWaypoint = m_path.vectorPath[m_targetWaypoint] - transform.position;

        // Check jump inputs
        bool jumpNeededToReachNextWaypoint = m_currentPathLink.type == PathLinkType.Jump || (m_currentPathLink.type == PathLinkType.DropDown && Mathf.Abs(m_currentPathLink.link.x) >= m_minDropDownWidthToJump);
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
        
        if (m_verticalMovementinProgress)
        {
            Debug.Log(Time.time + " " + Mathf.Abs(positionToTargetWaypoint.x));
            if (Mathf.Sign(positionToTargetWaypoint.x) == Mathf.Sign(m_horizontalInputForVerticalMovement) || Mathf.Abs(positionToTargetWaypoint.x) <= MinDistanceToMoveDuringVerticalMovement)
            {
                horizontalInput = m_horizontalInputForVerticalMovement;
            }
            else
            {
                horizontalInput = .0f;
            }
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
