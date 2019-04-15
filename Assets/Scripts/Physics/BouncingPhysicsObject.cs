using System.Collections.Generic;
using UnityEngine;

public class BouncingPhysicsObject : PhysicsObject
{
    [SerializeField]
    private Vector2 m_force = new Vector2(-10.0f, -2.0f);

    private void Start()
    {
        Velocity = m_force;
    }

    protected override void FixedUpdate()
    {
        // Update velocity
        //Velocity += CurrentGravityModifier * Physics2D.gravity * Time.fixedDeltaTime;
        //Velocity = new Vector2(m_targetHorizontalVelocity, Velocity.y);

        // Backup the list of gameObjects that used to collide and clear the original
        m_previouslyCollidingGameObject = new Dictionary<Collider2D, Rigidbody2D>(m_collidingGameObjects);
        m_collidingGameObjects.Clear();

        // Move
        Vector2 movement = Velocity * Time.fixedDeltaTime;
        Move(movement);

        /*Vector2 movement = movementAlongGround * deltaPosition.x;
        Move(movement, false);

        movement = Vector2.up * deltaPosition.y;
        Move(movement, true);*/

        // Check and broadcast collision exit message
        CheckCollisionExit();

        if (m_debugVelocity)
        {
            Debug.DrawLine(transform.position, transform.position + (Vector3)Velocity, Color.blue);
            Debug.DrawLine(transform.position, transform.position + (Vector3)movement, Color.red);
            Debug.DrawLine(transform.position, transform.position + Vector3.up * m_shellRadius, Color.yellow);
            Debug.Log(Velocity);
        }
    }

    protected override void Update()
    {
        /*m_targetHorizontalVelocity = .0f;
        ComputeVelocity();*/
    }

    private void Move(Vector2 movement)
    {
        Vector2 movementDirection = movement.normalized;
        float distanceToTravel = movement.magnitude;
        float distanceBeforeHit = movement.magnitude;

        // Keeps trying to move until the object travelled the necessary distance
        while(distanceToTravel > .0f)
        {
            // Consider that the objet can travel all the remining distance without hitting anything
            distanceBeforeHit = distanceToTravel;

            // Check for collision only if the object moves enough
            if (distanceToTravel > MinMoveDistance)
            {
                int count = m_rigidbody2D.Cast(movementDirection, m_contactFilter, m_hitBuffer, distanceToTravel + m_shellRadius);
                m_hitBufferList.Clear();

                // TEMP: Pourrais prendre juste le premier
                // Transfer hits to m_hitBufferList
                // m_hitBuffer has always the same number of elements in it, but some might be null.
                // The purpose of the transfer is to only keep the actual result of the cast
                for (int i = 0; i < count; i++)
                {
                    m_hitBufferList.Add(m_hitBuffer[i]);
                }

                foreach (RaycastHit2D hit in m_hitBufferList)
                {
                    // Update the movement direction
                    movementDirection = Vector2.Reflect(movementDirection, hit.normal);
                    Velocity = Velocity.magnitude * movementDirection;

                    // Check and broadcast collision enter message
                    CheckCollisionEnter(hit);
                    
                    // Calculate how much movement can be done, before hitting something, considering the ShellRadius  
                    distanceBeforeHit = hit.distance - m_shellRadius;
                    
                    // Will allow inheriting classes to add logic during the hit checks
                    OnColliderHitCheck(hit);

                    break;
                }
            }

            distanceToTravel -= distanceBeforeHit;

            // Apply the movement
            m_rigidbody2D.position = m_rigidbody2D.position + movementDirection * distanceBeforeHit;
        }
    }
}
