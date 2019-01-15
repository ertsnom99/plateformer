using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Rigidbody2D))]

public class FlyingMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float m_speed = 5.0f;

    [Header("Animation")]
    [SerializeField]
    private float m_minVelocityToRotatePropellant = 4.0f;
    [SerializeField]
    private float m_propellantRotationSpeed = 100.0f;
    [SerializeField]
    private bool m_flipSprite = false;

    [Header("Sprites")]
    [SerializeField]
    private SpriteRenderer m_bodySprite;
    [SerializeField]
    private SpriteRenderer m_propellantSprite;

    private Inputs m_currentInputs;

    private Rigidbody2D m_rigidbody;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();

        if (!m_bodySprite)
        {
            Debug.LogError("No sprite setted for the body!");
        }

        if (!m_propellantSprite)
        {
            Debug.LogError("No sprite setted for the propellant!");
        }
    }

    private void FixedUpdate()
    {
        m_rigidbody.AddForce(new Vector2(m_currentInputs.horizontal, m_currentInputs.vertical) * m_speed - m_rigidbody.velocity);

        Animate();
    }

    private void Animate()
    {
        float targetPropellantAngle = .0f;
        Vector3 originalPropellantRotation = m_propellantSprite.transform.localRotation.eulerAngles;

        // Flip the sprite if necessary
        bool flipSprite = false;
        
        flipSprite = (m_bodySprite.flipX == m_flipSprite ? (m_rigidbody.velocity.x < -.01f) : (m_rigidbody.velocity.x > .01f));
        
        if (flipSprite)
        {
            // Flip body
            m_bodySprite.flipX = !m_bodySprite.flipX;

            // Flip propellant
            Vector3 originalPropellantPosition = m_propellantSprite.transform.localPosition;
            m_propellantSprite.transform.localPosition = new Vector3(-originalPropellantPosition.x, originalPropellantPosition.y, originalPropellantPosition.z);
            m_propellantSprite.transform.rotation = Quaternion.Euler(originalPropellantRotation.x, originalPropellantRotation.y, -originalPropellantRotation.z);
            originalPropellantRotation = m_propellantSprite.transform.localRotation.eulerAngles;
        }
        
        // Adjust the rotation of the propellant
        // The propellant rotate only if their is enough movement
        if (m_rigidbody.velocity.magnitude >= m_minVelocityToRotatePropellant)
        {
            targetPropellantAngle = Mathf.Sign(-m_rigidbody.velocity.x) * Vector2.Angle(-m_rigidbody.velocity, Vector2.down);
        }
        
        Quaternion currentPropellantRotation = Quaternion.Euler(originalPropellantRotation);
        Quaternion targetPropellantRotation = Quaternion.Euler(originalPropellantRotation.x, originalPropellantRotation.y, targetPropellantAngle);

        // Rotate the propellant over time
        m_propellantSprite.transform.rotation = Quaternion.Lerp(currentPropellantRotation, targetPropellantRotation, m_propellantRotationSpeed * Time.deltaTime);
    }

    public void SetInputs(Inputs inputs)
    {
        m_currentInputs = inputs;
    }
}
