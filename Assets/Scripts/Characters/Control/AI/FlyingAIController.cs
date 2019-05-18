using UnityEngine;
using Pathfinding;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Seeker))]

public class FlyingAIController : AIController
{
    private FlyingMovement _movementScript;

    protected override void Awake()
    {
        base.Awake();

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
        if (HasDetectedTarget && Path != null)
        {
            Inputs inputs = NoControlInputs;

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

            // Send the final inputs to the movement script
            UpdateMovement(inputs);
        }
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
