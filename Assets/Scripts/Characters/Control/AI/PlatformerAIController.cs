using UnityEngine;
using Pathfinding;

public enum PathLinkType
{
    Walk = 0,
    Jump,
    DropDown
}

public struct PathLink
{
    public Vector2 Start;
    public Vector2 End;
    public Vector2 Link;
    public PathLinkType Type;

    public PathLink(Vector2 pStart, Vector2 pEnd, float minHeightForVerticalMovement)
    {
        Start = pStart;
        End = pEnd;
        Link = End - Start;

        if (Link.y >= minHeightForVerticalMovement)
        {
            Type = PathLinkType.Jump;
        }
        else if (Link.y <= -minHeightForVerticalMovement)
        {
            Type = PathLinkType.DropDown;
        }
        else
        {
            Type = PathLinkType.Walk;
        }
    }
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(Seeker))]

public class PlatformerAIController : AIController
{
    [Header("Vertical Movement")]
    [SerializeField]
    private bool _canJump = true;
    [SerializeField]
    private float _minHeightForVerticalMovement = 1.0f;
    [SerializeField]
    private float _minDropDownWidthToJump = 3.0f;
    [SerializeField]
    private float _minHeightToReleaseJump = 0.9f;
    private bool _jumpInputDown = false;

    private PathLink _currentPathLink;
    private bool _verticalMovementinProgress = false;
    private bool _delayedMovementProgressCheck = false;
    private float _horizontalInputForVerticalMovement = .0f;

    private PlatformerMovement _movementScript;

    private const float _jumpHorizontalMovementModifier = 1.2f;
    private const float _dropDownHorizontalMovementModifier = 1.0f;
    private const float _pathFixByAngleThreshold = .1f;
    private const float _minDistanceToMoveDuringVerticalMovement = 0.4f;

    protected override void Awake()
    {
        base.Awake();

        _movementScript = GetComponent<PlatformerMovement>();
    }

    // Called when a new path is created
    public override void OnPathComplete(Path path)
    {
        base.OnPathComplete(path);

        Path = path;
            
        // Fix the path has long has it needs to be
        while (Path.vectorPath.Count >= 3 && PathNeedsFix())
        {
            Path.vectorPath.RemoveAt(1);
            Debug.Log(Time.frameCount + " Fixed");
        }

        if (Path.vectorPath.Count > 1)
        {
            // Normally, the first point of the path SHOULD be extremely close to the position of the AI,
            // therefore it already reached the first waypoint which mean we can skip waypoint 0
            TargetWaypoint = 1;

            // Create the path link
            PathLink previousPathLink = _currentPathLink;
            _currentPathLink = new PathLink(Path.vectorPath[TargetWaypoint - 1], Path.vectorPath[TargetWaypoint], _minHeightForVerticalMovement);

            // Apply some adjustment needed to certain variables base on how the current path link and those variable were before the path was updated
            // For exemples:
            // -link got shorter and doens't have a big enough y to be created has a jump link
            // -Was a jump link, but suddenly became a drop down link

            // If the vertical movement didn't start and it is needed to reach the current waypoint
            if (!_verticalMovementinProgress && _currentPathLink.Type != PathLinkType.Walk)
            {
                _horizontalInputForVerticalMovement = CalculateHorizontalInputForVerticalMovement();
                _verticalMovementinProgress = true;
                _delayedMovementProgressCheck = _currentPathLink.Type == PathLinkType.Jump || _movementScript.IsGrounded;
            }
            else if (!_verticalMovementinProgress && _currentPathLink.Type == PathLinkType.Walk)
            {
                _delayedMovementProgressCheck = false;
            }
            // If the vertical movement did start, check if the PathLinkType might need to be adjust
            else if (_verticalMovementinProgress && _currentPathLink.Link.y > .0f)
            {
                if (_currentPathLink.Type != PathLinkType.Jump)
                {
                    _currentPathLink.Type = PathLinkType.Jump;
                }

                // Check if the PathLinkType pass from a drop down to a jump
                if (previousPathLink.Type == PathLinkType.DropDown)
                {
                    // TODO: Addapt calculation
                    _horizontalInputForVerticalMovement = CalculateHorizontalInputForVerticalMovement();
                    _delayedMovementProgressCheck = true;
                }
            }
            else if (_verticalMovementinProgress && _currentPathLink.Link.y < .0f)
            {
                if (_currentPathLink.Type != PathLinkType.DropDown)
                {
                    _currentPathLink.Type = PathLinkType.DropDown;
                }

                // Check if the PathLinkType pass from a jump to a drop down
                if (previousPathLink.Type == PathLinkType.Jump)
                {
                    _horizontalInputForVerticalMovement = CalculateHorizontalInputForVerticalMovement();
                    _delayedMovementProgressCheck = _movementScript.IsGrounded;
                }
            }
            // If the vertical movement did start, the path was updated at the same moment the AI touch the ground and the current path link is of a walk type
            else if (_verticalMovementinProgress && _movementScript.IsGrounded)
            {
                _verticalMovementinProgress = false;
                _delayedMovementProgressCheck = false;
            }
        }
    }

    private bool PathNeedsFix()
    {
        Vector2 secondToFirstWaypoint = Path.vectorPath[0] - Path.vectorPath[1];
        Vector2 secondToThirdWaypoint = Path.vectorPath[2] - Path.vectorPath[1];

        // Check if both vectors are along the exact same line and in the same direction
        return Vector2.Angle(secondToFirstWaypoint, secondToThirdWaypoint) <= _pathFixByAngleThreshold;
    }

    protected override void OnUpdateNotPossessed()
    {
        // Wait for the delay to end, before checking if the vertical movement is in progress 
        if (_delayedMovementProgressCheck && !_movementScript.IsGrounded && ((_currentPathLink.Link.y < .0f && _movementScript.Velocity.y < .0f) || (_currentPathLink.Link.y > .0f && _movementScript.Velocity.y > .0f)))
        {
            _delayedMovementProgressCheck = false;
        }
        else if (!_delayedMovementProgressCheck && _verticalMovementinProgress && _movementScript.IsGrounded)
        {
            _verticalMovementinProgress = false;
        }

        // The horizontal velocity might need to be adjusted
        if (_movementScript.IsGrounded && Mathf.Sign(_horizontalInputForVerticalMovement) != Mathf.Sign(_currentPathLink.Link.x))
        {
            _horizontalInputForVerticalMovement = CalculateHorizontalInputForVerticalMovement();
        }

        if (ControlsEnabled() && HasDetectedTarget && Path != null)
        {
            Inputs inputs = NoControlInputs;

            bool isWaypointReached = TargetWaypoint >= Path.vectorPath.Count;
            float distanceToTarget = Vector3.Distance(transform.position, Target.position);

            // Check if the AI hasn't reach either the target or the last waypoint
            if ((!StopWhenUnreachable || IsTargetReachable()) && !isWaypointReached && distanceToTarget > StopDistanceToTarget)
            {
                Vector2 positionToTargetWaypoint = Path.vectorPath[TargetWaypoint] - transform.position;

                // Update the target waypoint while the last one hasn't been reached and the current one has been past
                while (!isWaypointReached && (IsJumpOrDropDownLinkOver() || IsWalkLinkOver(positionToTargetWaypoint)))
                {
                    TargetWaypoint++;

                    isWaypointReached = TargetWaypoint >= Path.vectorPath.Count;

                    if (!isWaypointReached)
                    {
                        _currentPathLink = new PathLink(Path.vectorPath[TargetWaypoint - 1], Path.vectorPath[TargetWaypoint], _minHeightForVerticalMovement);
                        positionToTargetWaypoint = Path.vectorPath[TargetWaypoint] - transform.position;

                        // If a vertical movement is needed to reach the current waypoint, the horizontal input can be calculated immediatly
                        if (_currentPathLink.Type != PathLinkType.Walk)
                        {
                            _horizontalInputForVerticalMovement = CalculateHorizontalInputForVerticalMovement();
                            _verticalMovementinProgress = true;
                            _delayedMovementProgressCheck = _currentPathLink.Type == PathLinkType.Jump || _movementScript.IsGrounded;
                        }
                    }
                }

                // Create the inputs if the target waypoint still hasn't been reached
                if (!isWaypointReached)
                {
                    inputs = CreateInputs();
                }
            }

            // Send the final inputs to the movement script
            UpdateMovement(inputs);
        }
    }

    private bool IsWalkLinkOver(Vector2 positionToTargetWaypoint)
    {
        return _currentPathLink.Type == PathLinkType.Walk && (Vector2.Angle(_currentPathLink.Link, positionToTargetWaypoint) >= 90.0f || positionToTargetWaypoint.magnitude <= MinDistanceToChangeWaypoint);
    }

    private bool IsJumpOrDropDownLinkOver()
    {
        return (_currentPathLink.Type == PathLinkType.DropDown || _currentPathLink.Type == PathLinkType.Jump) && !_verticalMovementinProgress;
    }

    // Calculate how much time will be needed to complete the horizontal movement
    // Based on h=vi*t+(g*t^2)/2 formula, but solved to isolate t
    private float CalculateHorizontalInputForVerticalMovement()
    {
        float initialVelocity;
        float gravityUsed;
        float horizontalMovementModifier;

        // Use different values based on vertical movement direction
        if (_currentPathLink.Link.y >= .0f)
        {
            initialVelocity = _movementScript.JumpTakeOffSpeed;
            gravityUsed = _movementScript.CurrentGravityModifier * Physics2D.gravity.y;
            horizontalMovementModifier = _jumpHorizontalMovementModifier;
        }
        else
        {
            initialVelocity = .0f;
            gravityUsed = Mathf.Abs(_movementScript.CurrentGravityModifier * Physics2D.gravity.y);
            horizontalMovementModifier = _dropDownHorizontalMovementModifier;
        }

        float verticalMovement = Mathf.Abs(_currentPathLink.Link.y);
        float predictedTimeToReach = (Mathf.Sqrt(Mathf.Pow(initialVelocity, 2) + 2.0f * gravityUsed * verticalMovement) - initialVelocity) / gravityUsed;
        
        // Calculate how much distance/sec is need to cover the horizontal part of the movement
        float distanceBySecond = (_currentPathLink.Link.x * horizontalMovementModifier) / predictedTimeToReach;

        // Calculate how much of the horizontal speed is necessary and save it for later use
        return Mathf.Clamp(distanceBySecond / _movementScript.MaxSpeed, -1.0f, 1.0f);
    }

    protected override Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();

        if (UseKeyboard)
        {
            // Inputs from the keyboard
            //inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Jump = Input.GetButtonDown("Jump");
            inputs.ReleaseJump = Input.GetButtonUp("Jump");
            //inputs.Dash = Input.GetButtonDown("Dash");
            //inputs.ReleaseDash = Input.GetButtonUp("Dash");
            inputs.Possess = Input.GetButtonDown("Possess");
        }
        else
        {
            // TODO: Create inputs specific to the controler
            // Inputs from the controler
            //inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Jump = Input.GetButtonDown("Jump");
            inputs.ReleaseJump = Input.GetButtonUp("Jump");
            //inputs.Dash = Input.GetButtonDown("Dash");
            //inputs.ReleaseDash = Input.GetButtonUp("Dash");
            inputs.Possess = Input.GetButtonDown("Possess");
        }

        return inputs;
    }

    protected override Inputs CreateInputs()
    {
        Inputs inputs = NoControlInputs;

        Vector3 positionToTargetWaypoint = Path.vectorPath[TargetWaypoint] - transform.position;

        // Check jump inputs
        bool jumpNeededToReachNextWaypoint = _currentPathLink.Type == PathLinkType.Jump || (_currentPathLink.Type == PathLinkType.DropDown && Mathf.Abs(_currentPathLink.Link.x) >= _minDropDownWidthToJump);
        bool jump = _canJump && !_jumpInputDown && jumpNeededToReachNextWaypoint && _movementScript.IsGrounded;
        bool releaseJump = _jumpInputDown && (positionToTargetWaypoint.y <= -_minHeightToReleaseJump || _movementScript.Velocity.y < .0f);

        if (jump)
        {
            _jumpInputDown = true;
        }
        else if (releaseJump)
        {
            _jumpInputDown = false;
        }

        // The horizontal input must be calculate differently during a horizontal movement (before releasing the jump input in the case of a jump)
        float horizontalInput;
        
        if (_verticalMovementinProgress)
        {
            if (Mathf.Sign(positionToTargetWaypoint.x) == Mathf.Sign(_horizontalInputForVerticalMovement) || Mathf.Abs(positionToTargetWaypoint.x) <= _minDistanceToMoveDuringVerticalMovement)
            {
                horizontalInput = _horizontalInputForVerticalMovement;
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
        inputs.Horizontal = horizontalInput;
        inputs.Jump = jump;
        inputs.ReleaseJump = releaseJump;
        //inputs.dash = Input.GetButtonDown("Dash");
        //inputs.releaseDash = Input.GetButtonUp("Dash");

        return inputs;
    }

    protected override void UpdateMovement(Inputs inputs)
    {
        _movementScript.SetInputs(inputs);
    }

    protected override void OnPossess()
    {
        _verticalMovementinProgress = false;
        _delayedMovementProgressCheck = false;
    }

    protected override void OnUnpossess()
    {
        UpdateMovement(new Inputs());
    }
}
