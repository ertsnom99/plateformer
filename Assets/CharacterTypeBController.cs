using UnityEngine;

public class CharacterTypeBController : PhysicsObject
{
    [SerializeField]
    private float m_maxSpeed = 7.0f;
    [SerializeField]
    private float m_jumpTakeOffSpeed = 7.0f;

    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    protected override void Awake()
    {
        Time.timeScale = 0.2f;
        base.Awake();

        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
    }

    protected override void ComputeVelocity()
    {
        Vector2 move = Vector2.zero;

        move.x = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && m_isGrounded)
        {
            m_velocity.y = m_jumpTakeOffSpeed;
        }
        else if (Input.GetButtonUp("Jump"))
        {
            if (m_velocity.y > .0f)
            {
                m_velocity.y = m_velocity.y * 0.5f;
            }
        }

        m_targetVelocity = move * m_maxSpeed;

        bool flipSprite = (m_spriteRenderer.flipX ? (move.x > 0.01f) : (move.x < -0.01f));

        if (flipSprite)
        {
            m_spriteRenderer.flipX = !m_spriteRenderer.flipX;
        }

        m_animator.SetFloat("VelocityY", m_velocity.y);
        m_animator.SetBool("IsGrounded", m_isGrounded);
        m_animator.SetFloat("VelocityX", Mathf.Abs(m_velocity.x) / m_maxSpeed);
    }
}
