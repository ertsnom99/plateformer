using UnityEngine;
using Pathfinding;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Seeker))]

public class FlyingAIController : AIController
{
    [Header("Propellant")]
    [SerializeField]
    private Animator _propellantAnimator;

    private int _propellantPossessedModeAnimationLayerIndex;

    private FlyingMovement _movementScript;

    protected override void Awake()
    {
        base.Awake();

        if (!_propellantAnimator)
        {
            Debug.LogError("No propellant animator was set!");
        }
        else
        {
            _propellantPossessedModeAnimationLayerIndex = _propellantAnimator.GetLayerIndex(_possessedModeAnimationLayerName);
        }

        _movementScript = GetComponent<FlyingMovement>();
    }

    public override void OnPathComplete(Path path)
    {
        base.OnPathComplete(path);

        Path = path;
        TargetWaypoint = 0;
    }

    protected override void OnUpdateNotPossessed()
    {
        Inputs inputs = NoControlInputs;

        if (ControlsEnabled() && HasDetectedTarget && Path != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, Target.position);
            bool isWaypointReached = TargetWaypoint >= Path.vectorPath.Count;

            // Check if the AI hasn't reach either the target or the last waypoint
            if ((!StopWhenUnreachable || IsTargetReachable()) && distanceToTarget > StopDistanceToTarget && !isWaypointReached)
            {
                float distanceToWaypoint = Vector3.Distance(transform.position, Path.vectorPath[TargetWaypoint]);

                // Update the target waypoint while the last one hasn't been reached and the current one is close enough
                while (!isWaypointReached && distanceToWaypoint <= MinDistanceToChangeWaypoint)
                {
                    TargetWaypoint++;

                    isWaypointReached = TargetWaypoint >= Path.vectorPath.Count;

                    if (!isWaypointReached)
                    {
                        distanceToWaypoint = Vector3.Distance(transform.position, Path.vectorPath[TargetWaypoint]);
                    }

                }

                // Create the inputs if it wasn't done in the previous
                if (!isWaypointReached)
                {
                    inputs = CreateInputs();
                }
            }
        }

        // Send the final inputs to the movement script
        UpdateMovement(inputs);
    }

    protected override void OnPossess()
    {
        _propellantAnimator.SetLayerWeight(_propellantPossessedModeAnimationLayerIndex, 1.0f);
    }

    protected override void OnUnpossess()
    {
        _propellantAnimator.SetLayerWeight(_propellantPossessedModeAnimationLayerIndex, .0f);
    }

    protected override Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();

        if (UseKeyboard)
        {
            // Inputs from the keyboard
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            //inputs.Jump = Input.GetButtonDown("Jump");
            //inputs.ReleaseJump = Input.GetButtonUp("Jump");
            //inputs.Dash = Input.GetButtonDown("Dash");
            //inputs.ReleaseDash = Input.GetButtonUp("Dash");
            inputs.Possess = Input.GetButtonDown("Possess");
        }
        else
        {
            // TODO: Create inputs specific to the controler
            // Inputs from the controler
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            //inputs.Jump = Input.GetButtonDown("Jump");
            //inputs.ReleaseJump = Input.GetButtonUp("Jump");
            //inputs.Dash = Input.GetButtonDown("Dash");
            //inputs.ReleaseDash = Input.GetButtonUp("Dash");
            inputs.Possess = Input.GetButtonDown("Possess");
        }

        return inputs;
    }

    protected override Inputs CreateInputs()
    {
        Inputs inputs = NoControlInputs;

        Vector3 DirectionToTargetWaypoint = (Path.vectorPath[TargetWaypoint] - transform.position).normalized;

        // Inputs from the controler
        inputs.Vertical = DirectionToTargetWaypoint.y;
        inputs.Horizontal = DirectionToTargetWaypoint.x;
        //inputs.jump = jump;
        //inputs.releaseJump = releaseJump;
        //inputs.dash = Input.GetButtonDown("Dash");
        //inputs.releaseDash = Input.GetButtonUp("Dash");

        return inputs;
    }

    protected override void UpdateMovement(Inputs inputs)
    {
        _movementScript.SetInputs(inputs);
    }
}
