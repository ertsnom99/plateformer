using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class FlyingMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float m_speed = 5.0f;

    [Header("Animation")]
    [SerializeField]
    private bool m_flipSprite = false;

    private Inputs m_currentInputs;

    private Rigidbody2D m_rigidbody;
    private SpriteRenderer m_spriteRenderer;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        m_rigidbody.AddForce(new Vector2(m_currentInputs.horizontal, m_currentInputs.vertical) * m_speed - m_rigidbody.velocity);

        Animate();
    }

    private void Animate()
    {
        // Flip the sprite if necessary
        bool flipSprite = false;
        
        flipSprite = (m_spriteRenderer.flipX == m_flipSprite ? (m_rigidbody.velocity.x < -.01f) : (m_rigidbody.velocity.x > .01f));
        
        if (flipSprite)
        {
            m_spriteRenderer.flipX = !m_spriteRenderer.flipX;
        }
    }

    public void SetInputs(Inputs inputs)
    {
        m_currentInputs = inputs;
    }
}
