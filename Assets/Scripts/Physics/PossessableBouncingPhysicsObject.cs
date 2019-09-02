using UnityEngine;

public class PossessableBouncingPhysicsObject : BouncingPhysicsObject
{
    protected override void Move(Vector2 movement)
    {
        int tryCount = 0;
        float distanceToTravel = movement.magnitude;
        float distanceBeforeHit = movement.magnitude;
        Vector2 currentVelocityDirection = Velocity.normalized;

        // Keeps trying to move until the object travelled the necessary distance
        while (!MovementFrozen && tryCount <= BounceMaxTry && distanceToTravel > .0f)
        {
            // Consider that the objet can travel all the remining distance without hitting anything
            distanceBeforeHit = distanceToTravel;

            // Stock the direction of velocity since it is changed when there is a collision
            currentVelocityDirection = Velocity.normalized;

            // Check for collision only if the object moves enough
            if (distanceToTravel > MinMoveDistance)
            {
                // Cast and only consider the first collision
                int count = Collider.Cast(Velocity.normalized, ContactFilter, HitBuffer, distanceToTravel + ShellRadius);
                int index = 0;

                while (index < count && HitBuffer[index])
                {
                    PossessionPower possessionScript = HitBuffer[index].collider.GetComponent<PossessionPower>();

                    if (possessionScript && possessionScript.InPossessionMode)
                    {
                        if (!AllHitBufferList.Contains(HitBuffer[0]))
                        {
                            AllHitBufferList.Add(HitBuffer[0]);
                        }

                        // Will allow inheriting classes to add logic during the hit checks
                        OnColliderHitCheck(HitBuffer[index]);
                    }
                    else
                    {
                        if (!AllHitBufferList.Contains(HitBuffer[0]))
                        {
                            AllHitBufferList.Add(HitBuffer[0]);
                        }

                        // Update the velocity and the number of bounce
                        OnBounce(HitBuffer[index].normal);

                        if (!MovementFrozen)
                        {
                            // Stop all movement for a moment
                            StartCoroutine(FreezeMovementOnBounce());
                        }

                        // Update how much distance can be done before hitting something  
                        distanceBeforeHit = HitBuffer[index].distance - ShellRadius;

                        // Will allow inheriting classes to add logic during the hit checks
                        OnColliderHitCheck(HitBuffer[index]);

                        break;
                    }
                }
            }

            distanceToTravel -= distanceBeforeHit;

            // Apply the movement
            Rigidbody2D.position = Rigidbody2D.position + currentVelocityDirection * distanceBeforeHit;

            tryCount++;
        }

        // Rotate to be oriented toward Velocity direction
        float angle = Mathf.Atan2(Velocity.y, Velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
