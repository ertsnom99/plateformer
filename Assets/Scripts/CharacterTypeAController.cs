using UnityEngine;

public class CharacterTypeAController : MonoBehaviour
{
    [SerializeField]
    private float m_maxSpeed = 100.0f;
    [SerializeField]
    private Transform m_groundCheck;
    [SerializeField]
    private float m_groundRadius = .2f;
    [SerializeField]
    private LayerMask m_groundLayer;
    [SerializeField]
    private float m_jumpForce = 700.0f;

    private bool m_isFacingRight = true;
    private bool m_isGrounded = false;
    private float m_groundAngle;

    private Rigidbody2D m_rigidbody2D;
    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    private void Awake()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        m_groundAngle = .0f;
        m_isGrounded = Physics2D.OverlapCircle(m_groundCheck.position, m_groundRadius, m_groundLayer);

        float move = Mathf.Ceil(Input.GetAxis("Horizontal"));
        
        //Movement instantanious 
        m_rigidbody2D.AddForce(new Vector2(move * m_maxSpeed * Time.fixedDeltaTime, m_rigidbody2D.velocity.y) - m_rigidbody2D.velocity, ForceMode2D.Impulse);
        //Movement with acceleration/decceleration
        //m_rigidbody2D.AddForce(new Vector2(move * m_maxSpeed * Time.fixedDeltaTime, m_rigidbody2D.velocity.y) - m_rigidbody2D.velocity, ForceMode2D.Force);
        
        bool flipSprite = (m_spriteRenderer.flipX ? (move > 0.01f) : (move < -0.01f));

        if (flipSprite)
        {
            m_spriteRenderer.flipX = !m_spriteRenderer.flipX;
        }

        m_animator.SetFloat("VelocityX", Mathf.Abs(move));
        m_animator.SetBool("IsGrounded", m_isGrounded);
        m_animator.SetFloat("VelocityY", m_rigidbody2D.velocity.y);
        m_animator.SetFloat("GroundAngle", m_groundAngle);
    }

    private void Update()
    {
        if (m_isGrounded && Input.GetButtonDown("Jump"))
        {
            m_rigidbody2D.AddForce(new Vector2(.0f, m_jumpForce));

            m_isGrounded = false;
            m_animator.SetBool("IsGrounded", m_isGrounded);
        }
        else if (Input.GetButtonUp("Jump"))
        {
            if (m_rigidbody2D.velocity.y > .0f)
            {
                m_rigidbody2D.AddForce(new Vector2(m_rigidbody2D.velocity.x, m_rigidbody2D.velocity.y * 0.5f) - m_rigidbody2D.velocity, ForceMode2D.Impulse);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        foreach (ContactPoint2D contactPoint in col.contacts)
        {
            // update ground angle
            float angle = contactPoint.normal.x != .0f ? Vector2.Angle(contactPoint.normal, new Vector2(contactPoint.normal.x, .0f)) : 90.0f;

            if (angle > m_groundAngle)
            {
                m_groundAngle = angle;
            }
        }

        m_animator.SetFloat("GroundAngle", m_groundAngle);
    }

    private void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(m_groundCheck.position, m_groundRadius);
    }
}
