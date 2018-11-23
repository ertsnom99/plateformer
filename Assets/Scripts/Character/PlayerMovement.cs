using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public class PlayerMovement : PhysicsObject
{
    [SerializeField]
    private float m_maxSpeed = 6.6f;
    [SerializeField]
    private float m_jumpTakeOffSpeed = 15.0f;
    private bool m_triggeredJump = false;
    private bool m_jumpCanceled = false;

    private Vector2 m_lastTargetHorizontalVelocityDirection;

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
    private IEnumerator m_wallJumpWindowCoroutine;
    [SerializeField]
    private float m_horizontalControlDelayTime = .1f;
    private IEnumerator m_horizontalControlDelayCoroutine;

    [Header("Dash")]
    [SerializeField]
    private float m_dashSpeed = 15.0f;
    [SerializeField]
    private float m_dashDuration = .5f;
    private IEnumerator m_dashWindowCoroutine;
    [SerializeField]
    private float m_dashCooldown = 1.0f;
    private IEnumerator m_dashCooldownCoroutine;

    public bool IsDashing { get; private set; }

    [Header("Sound")]
    [SerializeField]
    private AudioClip m_jumpSound;
    [SerializeField]
    private AudioClip m_dashSound;

    private AudioSource m_audioSource;

    private Inputs m_currentInputs;

    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;

    protected int m_XVelocityParamHashId = Animator.StringToHash(XVelocityParamNameString);
    protected int m_YVelocityParamHashId = Animator.StringToHash(YVelocityParamNameString);
    protected int m_isGroundedParamHashId = Animator.StringToHash(IsGroundedParamNameString);
    protected int m_isSlidingOfWallParamHashId = Animator.StringToHash(IsSlidingOfWallParamNameString);
    protected int m_isDashingParamHashId = Animator.StringToHash(IsDashingParamNameString);

    public const string XVelocityParamNameString = "XVelocity";
    public const string YVelocityParamNameString = "YVelocity";
    public const string IsGroundedParamNameString = "IsGrounded";
    public const string IsSlidingOfWallParamNameString = "IsSlidingOfWall";
    public const string IsDashingParamNameString = "IsDashing";

    protected override void Awake()
    {
        base.Awake();

        IsSlidingOfWall = false;
        IsDashing = false;

        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
        m_audioSource = GetComponent<AudioSource>();
    }

    protected override void FixedUpdate()
    {
        // Keep track of the position of the player before it's updated
        Vector2 previousRigidbodyPosition = m_rigidbody2D.position;

        // Keep track of the direction the player intended to move in
        if (m_targetHorizontalVelocity != .0f)
        {
            m_lastTargetHorizontalVelocityDirection = new Vector2(m_targetHorizontalVelocity, .0f).normalized;
        }

        // Reset the Y velocity if the player is suppose to keep the same velocity while sliding of a wall
        if (IsSlidingOfWall && m_constantSlipeSpeed && m_velocity.y < .0f)
        {
            m_velocity.y = .0f;
        }

        base.FixedUpdate();

        // Reset jump flags used in Update
        m_triggeredJump = false;
        
        if (m_jumpCanceled)
        {
            m_jumpCanceled = !IsGrounded;
        }

        // Update dashing state base on the coroutine existence, since using the actual movement could create incorrect values!
        // This could happen because moving the rigidbody doesn't immediatly update the transform and wall slide detection doesn't
        // use the rigidbody position
        IsDashing = InDashWindow();

        UpdateWallSlide(m_rigidbody2D.position - previousRigidbodyPosition);

        // End the horizontal control delay if there is one and the player is grounded or sliding of a wall
        if (HorizontalControlDelayed() && (IsGrounded || IsSlidingOfWall))
        {
            EndDelayedHorizontalControl();
        }

        Animate();

        if (m_debugSlideRaycast)
        {
            Vector2 slideRaycastStart = new Vector2(transform.position.x, transform.position.y + m_slideRaycastOffset);
            Debug.DrawLine(slideRaycastStart, slideRaycastStart + m_lastTargetHorizontalVelocityDirection * m_slideRaycastDistance, Color.yellow);
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
    
    private void UpdateWallSlide(Vector2 movementDirection)
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
            if (!m_hitWall && movementDirection.x != .0f)
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

        // Reset m_hitWall flag for next check
        m_hitWall = false;
        
        // If the player started to slide of a new wall
        if (!wasSliding && IsSlidingOfWall)
        {
            m_velocity = Vector2.zero;

            // Cancel the other coroutines if necessary
            if (IsDashing)
            {
                EndDashWindow();
            }
            
            if (InWallJumpWindow())
            {
                EndWallJumpWindow();
            }
        }
        // If the player stopped to slide of a wall and not because of a dashing
        else if (wasSliding && !IsSlidingOfWall && !IsDashing)
        {
            m_wallJumpWindowCoroutine = WallJumpWindow();
            StartCoroutine(m_wallJumpWindowCoroutine);
        }

        // Update the gravity modifier when the player change his sliding state and not because of a dashing
        if (wasSliding != IsSlidingOfWall && !IsDashing)
        {
            m_currentGravityModifier = IsSlidingOfWall ? m_slideGravityModifier : m_gravityModifier;
        }
    }

    private bool RaycastForWallSlide()
    {
        Vector2 slideRaycastStart = new Vector2(transform.position.x, transform.position.y + m_slideRaycastOffset);

        RaycastHit2D[] results = new RaycastHit2D[1];
        Physics2D.Raycast(transform.position, m_lastTargetHorizontalVelocityDirection, m_contactFilter, results, m_slideRaycastDistance);
        
        return results[0].collider;
    }

    private void Animate()
    {
        // Flip the sprite if necessary
        bool flipSprite = false;

        if (!IsSlidingOfWall && !HorizontalControlDelayed())
        {
            flipSprite = (m_spriteRenderer.flipX ? (m_lastTargetHorizontalVelocityDirection.x > .01f) : (m_lastTargetHorizontalVelocityDirection.x < -.01f));
        }
        else if (IsSlidingOfWall)
        {
            flipSprite = (m_spriteRenderer.flipX ? (m_lastTargetHorizontalVelocityDirection.x < -.01f) : (m_lastTargetHorizontalVelocityDirection.x > .01f));
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
        m_animator.SetBool(m_isDashingParamHashId, IsDashing);
    }

    public void SetInputs(Inputs inputs)
    {
        m_currentInputs = inputs;
    }
    
    protected override void Update()
    {
        ComputeVelocity();
        PlaySounds();
    }

    protected override void ComputeVelocity()
    {
        // Only do either a jump or a dash during the next FixedUpdate
        if (!m_triggeredJump && !InDashWindow())
        {
            // Jump
            if (IsGrounded && m_currentInputs.jump)
            {
                Jump();
            }
            // Wall jump
            else if ((IsSlidingOfWall || InWallJumpWindow()) && m_currentInputs.jump)
            {
                WallJump();
            }
            // Dash
            else if (m_currentInputs.dash && !InDashWindow() && !DashInCooldown())
            {
                Dash();
            }
        }

        // Cancel jump if release jump button while having some jump velocity remaining
        if (!m_jumpCanceled && m_velocity.y > .0f && m_currentInputs.releaseJump)
        {
            CancelJump();
        }

        // Set the wanted horizontal velocity, except during the delayed controls window and the dash window
        if (!HorizontalControlDelayed() && !InDashWindow())
        {
            m_targetHorizontalVelocity = m_currentInputs.horizontal * m_maxSpeed;
        }
    }

    private void PlaySounds()
    {
        // Play sounds
        if (m_triggeredJump)
        {
        }
        else if (!IsDashing && InDashWindow())
        {
            m_audioSource.pitch = Random.Range(.9f, 1.0f);
            m_audioSource.PlayOneShot(m_dashSound);
        }
    }

    // Jump related methods
    private void Jump()
    {
        m_velocity.y = m_jumpTakeOffSpeed;

        // Reset ground normal because it is use for the movement along ground
        // If it's not reset, it could cause the player to move in a strange direction 
        m_groundNormal = new Vector2(.0f, 1.0f);

        // Update the gravity modifier
        m_currentGravityModifier = m_gravityModifier;

        m_triggeredJump = true;

        // Play jump sound
        m_audioSource.pitch = Random.Range(.9f, 1.0f);
        m_audioSource.PlayOneShot(m_jumpSound);
    }

    private IEnumerator WallJumpWindow()
    {
        yield return new WaitForSeconds(m_wallJumpWindowTime);
        m_wallJumpWindowCoroutine = null;
    }

    private void EndWallJumpWindow()
    {
        StopCoroutine(m_wallJumpWindowCoroutine);
        m_wallJumpWindowCoroutine = null;
    }
    
    private bool InWallJumpWindow()
    {
        return m_wallJumpWindowCoroutine != null;
    }

    private void WallJump()
    {
        Jump();
        m_targetHorizontalVelocity = m_spriteRenderer.flipX ? -m_wallJumpHorizontalVelocity : m_wallJumpHorizontalVelocity;

        m_horizontalControlDelayCoroutine = DelayHorizontalControl();
        StartCoroutine(m_horizontalControlDelayCoroutine);
    }

    private IEnumerator DelayHorizontalControl()
    {
        yield return new WaitForSeconds(m_horizontalControlDelayTime);
        m_horizontalControlDelayCoroutine = null;
    }

    private void EndDelayedHorizontalControl()
    {
        StopCoroutine(m_horizontalControlDelayCoroutine);
        m_horizontalControlDelayCoroutine = null;
    }

    private bool HorizontalControlDelayed()
    {
        return m_horizontalControlDelayCoroutine != null;
    }

    private void CancelJump()
    {
        m_velocity.y *= 0.5f;
        m_jumpCanceled = true;
    }

    // Dash related methods
    private void Dash()
    {
        m_velocity.y = .0f;
        m_targetHorizontalVelocity = m_spriteRenderer.flipX ? -m_dashSpeed : m_dashSpeed;

        // Reset ground normal because it is use for the movement along ground
        // If it's not reset, it could cause the player to move in a strange direction 
        m_groundNormal = new Vector2(.0f, 1.0f);

        // Update the gravity modifier
        m_currentGravityModifier = .0f;

        // Play dash sound
        m_audioSource.pitch = Random.Range(.9f, 1.0f);
        m_audioSource.PlayOneShot(m_dashSound);

        // Cancel the other coroutines if necessary
        if (InWallJumpWindow())
        {
            EndWallJumpWindow();
        }

        if (HorizontalControlDelayed())
        {
            EndDelayedHorizontalControl();
        }

        m_dashWindowCoroutine = UseDashWindow();
        StartCoroutine(m_dashWindowCoroutine);
    }

    private IEnumerator UseDashWindow()
    {
        yield return new WaitForSeconds(m_dashDuration);
        m_dashWindowCoroutine = null;

        // Update the gravity modifier
        m_currentGravityModifier = m_gravityModifier;

        m_dashCooldownCoroutine = DashCooldown();
        StartCoroutine(m_dashCooldownCoroutine);
    }

    private void EndDashWindow()
    {
        StopCoroutine(m_dashWindowCoroutine);
        m_dashWindowCoroutine = null;

        // Update the gravity modifier
        m_currentGravityModifier = m_gravityModifier;

        IsDashing = false;

        m_dashCooldownCoroutine = DashCooldown();
        StartCoroutine(m_dashCooldownCoroutine);
    }

    private bool InDashWindow()
    {
        return m_dashWindowCoroutine != null;
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(m_dashCooldown);
        m_dashCooldownCoroutine = null;  
    }

    private bool DashInCooldown()
    {
        return m_dashCooldownCoroutine != null;
    }
}
