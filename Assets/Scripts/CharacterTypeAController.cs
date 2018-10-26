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

    private Rigidbody2D m_rigidbody2D;
    private Animator m_animator;

    private void Awake()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        m_isGrounded = Physics2D.OverlapCircle(m_groundCheck.position, m_groundRadius, m_groundLayer);

        float move = Mathf.Ceil(Input.GetAxis("Horizontal"));
        
        //Movement instantanious 
        m_rigidbody2D.AddForce(new Vector2(move * m_maxSpeed * Time.fixedDeltaTime, m_rigidbody2D.velocity.y) - m_rigidbody2D.velocity, ForceMode2D.Impulse);
        //m_rigidbody2D.velocity = new Vector2(move * m_maxSpeed * Time.fixedDeltaTime, m_rigidbody2D.velocity.y);
        //Movement with acceleration/decceleration
        //m_rigidbody2D.AddForce(new Vector2(move * m_maxSpeed * Time.fixedDeltaTime, m_rigidbody2D.velocity.y) - m_rigidbody2D.velocity, ForceMode2D.Force);
        
        if (move > .0f && !m_isFacingRight)
        {
            Flip();
        }
        else if (move < .0f && m_isFacingRight)
        {
            Flip();
        }

        m_animator.SetFloat("Speed", Mathf.Abs(move));
        m_animator.SetBool("IsGrounded", m_isGrounded);
        m_animator.SetFloat("VerticalSpeed", m_rigidbody2D.velocity.y);
    }

    private void Update()
    {
        if (m_isGrounded && Input.GetButtonDown("Jump"))
        {
            m_rigidbody2D.AddForce(new Vector2(.0f, m_jumpForce));

            m_isGrounded = false;
            m_animator.SetBool("IsGrounded", m_isGrounded);
        }
    }

    private void Flip()
    {
        m_isFacingRight = !m_isFacingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    private void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(m_groundCheck.position, m_groundRadius);
    }
}
