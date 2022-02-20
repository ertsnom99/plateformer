using Pathfinding;
using UnityEngine;
using UnityEngine.InputSystem;
//using Pathfinding;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(FlyingMovement))]

public class FlyingCharacterController : PossessableCharacterController
{
    private FlyingMovement _movementScript;

    protected override void Awake()
    {
        base.Awake();

        _movementScript = GetComponent<FlyingMovement>();
    }

    protected override void OnUpdatePossessed()
    {
        // TODO: Fix this
        /*if (ControlsEnabled())
        {
            UpdateDisplayInfo(PossessingController.CurrentInputs);
            UpdateMovement(PossessingController.CurrentInputs);
            UpdatePossession(PossessingController.CurrentInputs);
        }
        else
        {
            UpdateDisplayInfo(NoControlInputs);
            UpdateMovement(NoControlInputs);
            UpdatePossession(NoControlInputs);
        }*/
    }

    protected override void OnUpdateNotPossessed()
    {
        base.OnUpdateNotPossessed();

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

    protected override Inputs CreateInputs()
    {
        Inputs inputs = NoControlInputs;

        Vector3 DirectionToTargetWaypoint = (Path.vectorPath[TargetWaypoint] - transform.position).normalized;

        // Inputs from the controler
        inputs.Horizontal = DirectionToTargetWaypoint.x;
        inputs.Vertical = DirectionToTargetWaypoint.y;

        return inputs;
    }

    private void UpdateMovement(Inputs inputs)
    {
        _movementScript.SetInputs(inputs);
    }
}
