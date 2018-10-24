using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField]
    private float m_maxSpeed = 100.0f;

    private bool m_isFacingRight = true;

    private Rigidbody2D m_rigidbody2D;
    private Animator m_animator;

    private void Awake()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        float move = Input.GetAxis("Horizontal");
        
        m_rigidbody2D.velocity = new Vector2(move * m_maxSpeed * Time.fixedDeltaTime, m_rigidbody2D.velocity.y);

        if (move > .0f && !m_isFacingRight)
        {
            Flip();
        }
        else if (move < .0f && m_isFacingRight)
        {
            Flip();
        }

        m_animator.SetFloat("Speed", Mathf.Abs(move));
    }

    private void Flip()
    {
        m_isFacingRight = !m_isFacingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }
}
