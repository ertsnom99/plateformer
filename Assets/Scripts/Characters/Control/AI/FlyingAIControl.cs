using UnityEngine;
using Pathfinding;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Seeker))]

public class FlyingAIControl : AIControl
{
    private FlyingMovement m_movementScript;

    protected override void Awake()
    {
        base.Awake();

        m_movementScript = GetComponent<FlyingMovement>();
    }

    public override void OnPathComplete(Path path)
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
                        while (!isWaypointReached && distanceToWaypoint <= m_minDistanceToChangeWaypoint)
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

    protected override Inputs CreateInputs()
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

    protected override void UpdateMovement(Inputs inputs)
    {
        m_movementScript.SetInputs(inputs);
    }
}
