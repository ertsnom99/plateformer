using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]

public class PlayerMovement : PhysicsObject
{
    [SerializeField]
    private float m_maxSpeed = 6.6f;
    [SerializeField]
    private float m_jumpTakeOffSpeed = 15.0f;

    private Vector2 m_lastIntendedMovementDirection;

    [Header("Slide of wall")]
    [SerializeField]
    private bool m_canSlideGoingUp = false;
    [SerializeField]
    private float m_slideRaycastOffset = -.45f;
    [SerializeField]
    private float m_slideRaycastDistance = .6f;
    [SerializeField]
    private float m_slideGravityModifier = .2f;
    [SerializeField]
    private bool m_constantSlipeSpeed = false;

    private bool m_hitWall = false;
    private Vector2 m_lastHitWallNormal = Vector2.zero;

    public bool IsSlidingOfWall { get; private set; }

    [SerializeField]
    private bool m_debugSlideRaycast = false;

    [Header("Wall jump")]
    [SerializeField]
    private float m_wallJumpHorizontalVelocity = 6.0f;
    [SerializeField]
    private float m_wallJumpWindowTime = .05f;
    private bool m_inWallJumpWindow = false;
    private IEnumerator m_wallJumpWindowCoroutine;
    [SerializeField]
    private float m_horizontalControlDelayTime = .1f;
    private bool m_isHorizontalControlDelayed = false;
    private IEnumerator m_horizontalControlDelayCoroutine;

    private Inputs m_currentInputs;

    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    protected int m_XVelocityParamHashId = Animator.StringToHash(XVelocityParamNameString);
    protected int m_YVelocityParamHashId = Animator.StringToHash(YVelocityParamNameString);
    protected int m_isGroundedParamHashId = Animator.StringToHash(IsGroundedParamNameString);
    protected int m_isSlidingOfWallParamHashId = Animator.StringToHash(IsSlidingOfWallParamNameString);

    public const string XVelocityParamNameString = "XVelocity";
    public const string YVelocityParamNameString = "YVelocity";
    public const string IsGroundedParamNameString = "IsGrounded";
    public const string IsSlidingOfWallParamNameString = "IsSlidingOfWall";

    protected override void Awake()
    {
        base.Awake();

        IsSlidingOfWall = false;

        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
    }

    protected override void FixedUpdate()
    {
        // Keep track of the position of the player before it's updated
        Vector2 previousRigidbodyPosition = m_rigidbody2D.position;

        // Reset the Y velocity if the player is suppose to keep the same velocity while sliding of a wall
        if (IsSlidingOfWall && m_constantSlipeSpeed && m_velocity.y < .0f)
        {
            m_velocity.y = .0f;
        }

        // Keep track of direction the player is facing
        if (m_targetHorizontalVelocity != .0f)
        {
            m_lastIntendedMovementDirection = new Vector2(m_targetHorizontalVelocity, .0f);
        }

        base.FixedUpdate();

        CheckForWallSlide(m_rigidbody2D.position - previousRigidbodyPosition);

        // End the horizontal control delay if there is one and the player is grounded or sliding of a wall
        if (m_isHorizontalControlDelayed && (IsGrounded || IsSlidingOfWall))
        {
            EndDelayHorizontalControl();
        }

        Animate();

        if (m_debugSlideRaycast)
        {
            Vector2 slideRaycastStart = new Vector2(transform.position.x, transform.position.y + m_slideRaycastOffset);
            Debug.DrawLine(slideRaycastStart, slideRaycastStart + m_lastIntendedMovementDirection.normalized * m_slideRaycastDistance, Color.yellow);
        }
    }

    protected override void OnColliderHitCheck(RaycastHit2D hit)
    {
        // Set flag to tell that the character hit a wall
        if (hit.normal.x != .0f && hit.normal.y == .0f)
        {
            m_hitWall = true;
            m_lastHitWallNormal = hit.normal;
        }
    }

    private void CheckForWallSlide(Vector2 movementDone)
    {
        // Flag to check later if the player started to slide of a wall
        bool wasSliding = IsSlidingOfWall;

        // Just touched ground after a slide of a wall
        if (IsSlidingOfWall && IsGrounded)
        {
            IsSlidingOfWall = false;
        }
        // Might be starting to slide of a wall
        else if ((m_canSlideGoingUp || m_velocity.y <= .0f) && m_hitWall && !IsSlidingOfWall && !IsGrounded)
        {
            // Check if the player can slide of the wall
            IsSlidingOfWall = RaycastForWallSlide();
        }
        // Might need to stop sliding of a wall
        else if (IsSlidingOfWall)
        {
            // If the player moves away from wall
            if (!m_hitWall && (movementDone).x != .0f)
            {
                IsSlidingOfWall = false;
            }
            // Check if the player finished sliding off a wall
            else
            {
                // Check if the player can slide of the wall
                IsSlidingOfWall = RaycastForWallSlide();
            }
        }

        // If the player started to slide of a new wall
        if (!wasSliding && IsSlidingOfWall)
        {
            m_velocity = Vector2.zero;
            
            // Cancel the wall jump window if necessary
            if (m_inWallJumpWindow)
            {
                EndWallJumpWindow();
            }
        }
        // If the player stopped to slide of a wall
        else if (wasSliding && !IsSlidingOfWall)
        {
            m_wallJumpWindowCoroutine = WallJumpWindow();
            StartCoroutine(m_wallJumpWindowCoroutine);
        }
        
        // Update the gravity modifier
        m_currentGravityModifier = IsSlidingOfWall ? m_slideGravityModifier : m_gravityModifier;

        // Reset m_hitWall flag for next check
        m_hitWall = false;
    }

    private bool RaycastForWallSlide()
    {
        Vector2 slideRaycastStart = new Vector2(transform.position.x, transform.position.y + m_slideRaycastOffset);

        RaycastHit2D[] results = new RaycastHit2D[1];
        Physics2D.Raycast(transform.position, m_lastIntendedMovementDirection, m_contactFilter, results, m_slideRaycastDistance);

        return results[0].collider;
    }

    private void Animate()
    {
        // Flip the sprite if necessary
        bool flipSprite = false;

        if (!IsSlidingOfWall && !m_isHorizontalControlDelayed)
        {
            flipSprite = (m_spriteRenderer.flipX ? (m_lastIntendedMovementDirection.x > .01f) : (m_lastIntendedMovementDirection.x < -.01f));
        }
        else if (IsSlidingOfWall)
        {
            flipSprite = (m_spriteRenderer.flipX ? (m_lastIntendedMovementDirection.x < -.01f) : (m_lastIntendedMovementDirection.x > .01f));
        }

        if (flipSprite)
        {
            m_spriteRenderer.flipX = !m_spriteRenderer.flipX;
        }

        // Update animator parameters
        m_animator.SetFloat(m_XVelocityParamHashId, Mathf.Abs(m_velocity.x) / m_maxSpeed);
        m_animator.SetFloat(m_YVelocityParamHashId, m_velocity.y);
        m_animator.SetBool(IsGroundedParamNameString, IsGrounded);
        m_animator.SetBool(m_isSlidingOfWallParamHashId, IsSlidingOfWall);
    }

    public void SetInputs(Inputs inputs)
    {
        m_currentInputs = inputs;
    }
    
    protected override void Update()
    {
        ComputeVelocity();
    }

    protected override void ComputeVelocity()
    {
        // Jump
        if (IsGrounded && m_currentInputs.jump)
        {
            Jump();
        }
        // Wall jump
        else if ((IsSlidingOfWall || m_inWallJumpWindow) && m_currentInputs.jump)
        {
            WallJump();
        }
        // Cancel jump if release jump button while having some jump velocity remaining
        else if (m_velocity.y > .0f && m_currentInputs.releaseJump)
        {
            CancelJump();
        }

        // Set the wanted horizontal velocity
        if (!m_isHorizontalControlDelayed)
        {
            m_targetHorizontalVelocity = m_currentInputs.horizontal * m_maxSpeed;
        }
    }

    private void Jump()
    {
        m_velocity.y = m_jumpTakeOffSpeed;
        m_groundNormal = new Vector2(.0f, 1.0f);

        // Update the gravity modifier
        if (m_currentGravityModifier != m_gravityModifier)
        {
            m_currentGravityModifier = m_gravityModifier;
        }
    }

    private void WallJump()
    {
        Jump();
        m_targetHorizontalVelocity = m_spriteRenderer.flipX ? -m_wallJumpHorizontalVelocity : m_wallJumpHorizontalVelocity;

        m_horizontalControlDelayCoroutine = DelayHorizontalControl();
        StartCoroutine(m_horizontalControlDelayCoroutine);
    }

    private IEnumerator WallJumpWindow()
    {
        m_inWallJumpWindow = true;

        yield return new WaitForSeconds(m_wallJumpWindowTime);

        m_wallJumpWindowCoroutine = null;
        m_inWallJumpWindow = false;
    }

    private void EndWallJumpWindow()
    {
        StopCoroutine(m_wallJumpWindowCoroutine);
        m_wallJumpWindowCoroutine = null;

        m_inWallJumpWindow = false;
    }

    private IEnumerator DelayHorizontalControl()
    {
        m_isHorizontalControlDelayed = true;

        yield return new WaitForSeconds(m_horizontalControlDelayTime);

        m_horizontalControlDelayCoroutine = null;
        m_isHorizontalControlDelayed = false;
    }

    private void EndDelayHorizontalControl()
    {
        StopCoroutine(m_horizontalControlDelayCoroutine);
        m_horizontalControlDelayCoroutine = null;

        m_isHorizontalControlDelayed = false;
    }

    private void CancelJump()
    {
        m_velocity.y *= 0.5f;
    }
}
