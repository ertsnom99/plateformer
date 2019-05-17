using UnityEngine;

public class CharacterTypeAController : MonoBehaviour
{
    [SerializeField]
    private float _maxSpeed = 100.0f;
    [SerializeField]
    private Transform _groundCheck;
    [SerializeField]
    private float _groundRadius = .2f;
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private float _jumpForce = 700.0f;
    
    public bool IsGrounded { get; private set; }
    private float _groundAngle;

    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        IsGrounded = false;
    }

    private void FixedUpdate()
    {
        _groundAngle = .0f;
        IsGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundRadius, _groundLayer);

        float move = Mathf.Ceil(Input.GetAxis("Horizontal"));
        
        //Movement instantanious 
        _rigidbody2D.AddForce(new Vector2(move * _maxSpeed * Time.fixedDeltaTime, _rigidbody2D.velocity.y) - _rigidbody2D.velocity, ForceMode2D.Impulse);
        //Movement with acceleration/decceleration
        //m_rigidbody2D.AddForce(new Vector2(move * m_maxSpeed * Time.fixedDeltaTime, m_rigidbody2D.velocity.y) - m_rigidbody2D.velocity, ForceMode2D.Force);
        
        bool flipSprite = (_spriteRenderer.flipX ? (move > 0.01f) : (move < -0.01f));

        if (flipSprite)
        {
            _spriteRenderer.flipX = !_spriteRenderer.flipX;
        }

        _animator.SetFloat("XVelocity", Mathf.Abs(move));
        _animator.SetBool("IsGrounded", IsGrounded);
        _animator.SetFloat("YVelocity", _rigidbody2D.velocity.y);
        _animator.SetFloat("GroundAngle", _groundAngle);
    }

    private void Update()
    {
        if (IsGrounded && Input.GetButtonDown("Jump"))
        {
            _rigidbody2D.AddForce(new Vector2(.0f, _jumpForce) - new Vector2(.0f, _rigidbody2D.velocity.y), ForceMode2D.Impulse);

            IsGrounded = false;
            _animator.SetBool("IsGrounded", IsGrounded);
        }
        else if (Input.GetButtonUp("Jump"))
        {
            if (_rigidbody2D.velocity.y > .0f)
            {
                _rigidbody2D.AddForce(new Vector2(_rigidbody2D.velocity.x, _rigidbody2D.velocity.y * 0.5f) - new Vector2(.0f, _rigidbody2D.velocity.y), ForceMode2D.Impulse);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        foreach (ContactPoint2D contactPoint in col.contacts)
        {
            // update ground angle
            float angle = contactPoint.normal.x != .0f ? Vector2.Angle(contactPoint.normal, new Vector2(contactPoint.normal.x, .0f)) : 90.0f;

            if (angle > _groundAngle)
            {
                _groundAngle = angle;
            }
        }

        _animator.SetFloat("GroundAngle", _groundAngle);
    }

    private void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundRadius);
    }
}
