using System.Collections;
using UnityEngine;

public interface IPlatformerMovementSubscriber
{
    void NotifyDashUsed();
    void NotifyDashCooldownUpdated(float cooldownProgress);
    void NotifyDashCooldownOver();
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public class PlatformerMovement : SubscribablePhysicsObject<IPlatformerMovementSubscriber>
{
    [SerializeField]
    private float m_maxSpeed = 6.6f;

    public float MaxSpeed
    {
        get { return m_maxSpeed; }
        private set { m_maxSpeed = value; }
    }

    [Header("Jump")]
    [SerializeField]
    private bool m_canJump = true;

    [SerializeField]
    private float m_jumpTakeOffSpeed = 15.0f;

    public float JumpTakeOffSpeed
    {
        get { return m_jumpTakeOffSpeed; }
        private set { m_jumpTakeOffSpeed = value; }
    }

    private bool m_triggeredJump = false;
    private bool m_jumpCanceled = false;
    
    private float m_lastHorizontalVelocityDirection;
    
    public bool IsKnockedBack { get; private set; }

    [Header("Airborne jump")]
    [SerializeField]
    private bool m_canAirborneJump = false;
    [SerializeField]
    private float m_airborneJumpTakeOffSpeed = 10.0f;
    private bool m_airborneJumpAvailable = true;
    private bool m_triggeredAirborneJump = false;

    // NOTE: The current implementation would cause problems if m_canSlideOfWall was changed during a wall slide
    [Header("Slide of wall")]
    [SerializeField]
    private bool m_canSlideOfWall = false;
    [SerializeField]
    private bool m_canSlideGoingUp = false;
    [SerializeField]
    private bool m_canSlideDuringKnockBack = false;
    [SerializeField]
    private float m_slideRaycastOffset = -.45f;
    [SerializeField]
    private float m_slideRaycastDistance = .6f;
    [SerializeField]
    private float m_slideGravityModifier = .2f;
    [SerializeField]
    private bool m_constantSlipeSpeed = false;

    private bool m_hitWall = false;

    public bool IsSlidingOfWall { get; private set; }

    [SerializeField]
    private bool m_debugSlideRaycast = false;

    // NOTE: The current implementation needs wall slide to be enable for the wall jump to be used 
    [Header("Wall jump")]
    [SerializeField]
    private bool m_canWallJump = false;
    [SerializeField]
    private float m_wallJumpHorizontalVelocity = 6.0f;
    [SerializeField]
    private float m_wallJumpWindowTime = .05f;
    private IEnumerator m_wallJumpWindowCoroutine;
    [SerializeField]
    private float m_WallJumpControlDelayTime = .1f;
    private IEnumerator m_horizontalControlDelayCoroutine;

    [Header("Dash")]
    [SerializeField]
    private bool m_canDash = false;
    [SerializeField]
    private float m_dashSpeed = 15.0f;
    [SerializeField]
    private float m_dashDuration = .5f;
    [SerializeField]
    private float m_dashCooldown = 1.0f;
    private IEnumerator m_dashWindowCoroutine;
    private IEnumerator m_dashCooldownCoroutine;
    private bool m_triggeredDash = false;

    public bool IsDashing { get; private set; }

    [Header("Animation")]
    [SerializeField]
    private bool m_flipSprite = false;
    // MAYBE: Flags used to delay animations
    //private bool m_wasDashAnimeDelayed = false;

    [Header("Sound")]
    [SerializeField]
    private AudioClip m_jumpSound;
    [SerializeField]
    private AudioClip m_airborneJumpSound;
    [SerializeField]
    private AudioClip m_dashSound;
    
    private Inputs m_currentInputs;

    private SpriteRenderer m_spriteRenderer;
    private Animator m_animator;
    private AudioSource m_audioSource;

    protected int m_XVelocityParamHashId = Animator.StringToHash(XVelocityParamNameString);
    protected int m_YVelocityParamHashId = Animator.StringToHash(YVelocityParamNameString);
    protected int m_isGroundedParamHashId = Animator.StringToHash(IsGroundedParamNameString);
    protected int m_isSlidingOfWallParamHashId = Animator.StringToHash(IsSlidingOfWallParamNameString);
    protected int m_isDashingParamHashId = Animator.StringToHash(IsDashingParamNameString);
    protected int m_airborneJumpParamHashId = Animator.StringToHash(AirborneJumpParamNameString);
    protected int m_knockedBackParamHashId = Animator.StringToHash(KnockedBackParamNameString);

    public const string XVelocityParamNameString = "XVelocity";
    public const string YVelocityParamNameString = "YVelocity";
    public const string IsGroundedParamNameString = "IsGrounded";
    public const string IsSlidingOfWallParamNameString = "IsSlidingOfWall";
    public const string IsDashingParamNameString = "IsDashing";
    public const string AirborneJumpParamNameString = "AirborneJump";
    public const string KnockedBackParamNameString = "KnockedBack";

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
            m_lastHorizontalVelocityDirection = Mathf.Sign(m_targetHorizontalVelocity);
        }

        // Reset the Y velocity if the player is suppose to keep the same velocity while sliding of a wall
        if (IsSlidingOfWall && m_constantSlipeSpeed && m_velocity.y < .0f)
        {
            m_velocity.y = .0f;
        }

        base.FixedUpdate();

        // Stop the dash window if an airborne jump was triggered during a dash
        if (InDashWindow() && m_triggeredAirborneJump)
        {
            EndDashWindow();
        }

        // Update dashing state before UpdateWallSlide(), since it uses that flag
        IsDashing = InDashWindow();

        // Only update wall slide if it's allowed
        if (m_canSlideOfWall)
        {
            UpdateWallSlide(m_rigidbody2D.position - previousRigidbodyPosition);
        }

        // Cancel dash window if hitted a wall
        if (InDashWindow() && m_hitWall)
        {
            EndDashWindow();
        }

        // Reset flags if they are in a certain state
        // While the airborne jump isn't available, keep it unavailable has long has the character isn't grounded and isn't sliding of a wall
        if (!m_airborneJumpAvailable)
        {
            m_airborneJumpAvailable = IsGrounded || IsSlidingOfWall;
        }

        // While the jump was canceled, keep it canceled has long has the character isn't grounded and isn't sliding of a wall
        if (m_jumpCanceled)
        {
            m_jumpCanceled = !IsGrounded && !IsSlidingOfWall;
        }

        // End the horizontal control delay if there is one, except for the knock back, and the player is grounded or sliding of a wall
        if (!IsKnockedBack && HorizontalControlDelayed() && (IsGrounded || IsSlidingOfWall))
        {
            EndDelayedHorizontalControl();
        }

        // Animate before resetting flags, since some of those flags are necessary for Animate()
        Animate();
        
        // Reset m_hitWall flag for next check
        m_hitWall = false;

        // Reset flags used in Update
        m_triggeredJump = false;
        m_triggeredAirborneJump = false;
        m_triggeredDash = false;

        // Debug
        if (m_debugSlideRaycast)
        {
            Vector2 slideRaycastStart = new Vector2(transform.position.x, transform.position.y + m_slideRaycastOffset);
            Debug.DrawLine(slideRaycastStart, slideRaycastStart + m_lastHorizontalVelocityDirection * Vector2.right * m_slideRaycastDistance, Color.yellow);
        }
    }

    protected override void OnColliderHitCheck(RaycastHit2D hit)
    {
        // Set flag to tell that the character hit a wall
        if (hit.normal.x != .0f && hit.normal.y == .0f)
        {
            m_hitWall = true;

            // Reset the target velocity when the character is knocked back because it is suppose to loose it's velocity when that happens
            if (IsKnockedBack)
            {
                m_targetHorizontalVelocity = .0f;
            }
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
        else if ((m_canSlideDuringKnockBack || !IsKnockedBack) && (m_canSlideGoingUp || m_velocity.y <= .0f) && m_hitWall && !IsSlidingOfWall && !IsGrounded)
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
                IsSlidingOfWall = RaycastForWallSlide();

                if (!IsSlidingOfWall)
                {
                    // Reverse direction to prevent the character from facing the wrong way
                    m_lastHorizontalVelocityDirection = -m_lastHorizontalVelocityDirection;
                }
            }
        }
        
        // If the player started to slide of a new wall
        if (!wasSliding && IsSlidingOfWall)
        {
            m_velocity = Vector2.zero;

            // Cancel the other coroutines if necessary
            if (IsDashing)
            {
                EndDashWindow();
                IsDashing = false;
            }
            
            if (InWallJumpWindow())
            {
                EndWallJumpWindow();
            }
        }
        // If the player stopped to slide of a wall and not because he did a wall jump or he dashed 
        else if (wasSliding && !IsSlidingOfWall && !m_triggeredJump && !IsDashing)
        {
            m_wallJumpWindowCoroutine = WallJumpWindow();
            StartCoroutine(m_wallJumpWindowCoroutine);
        }

        // Update the gravity modifier when the player change his sliding state and not because he dashed
        if (wasSliding != IsSlidingOfWall && !IsDashing)
        {
            m_currentGravityModifier = IsSlidingOfWall ? m_slideGravityModifier : m_gravityModifier;
        }
    }

    private bool RaycastForWallSlide()
    {
        Vector2 slideRaycastStart = new Vector2(m_rigidbody2D.position.x, m_rigidbody2D.transform.position.y + m_slideRaycastOffset);

        RaycastHit2D[] results = new RaycastHit2D[1];
        Physics2D.Raycast(slideRaycastStart, m_lastHorizontalVelocityDirection * Vector2.right, m_contactFilter, results, m_slideRaycastDistance);

        return results[0].collider;
    }

    private void Animate()
    {
        // Flip the sprite if necessary
        bool flipSprite = false;
        
        if (!IsSlidingOfWall && !HorizontalControlDelayed())
        {
            flipSprite = (m_spriteRenderer.flipX == m_flipSprite ? (m_lastHorizontalVelocityDirection < .0f) : (m_lastHorizontalVelocityDirection > .0f));
        }
        else if (IsSlidingOfWall || IsKnockedBack)
        {
            flipSprite = (m_spriteRenderer.flipX == m_flipSprite ? (m_lastHorizontalVelocityDirection > .0f) : (m_lastHorizontalVelocityDirection < .0f));
        }
        
        if (flipSprite)
        {
            m_spriteRenderer.flipX = !m_spriteRenderer.flipX;
        }

        // Update animator parameters
        m_animator.SetFloat(m_XVelocityParamHashId, Mathf.Abs(m_velocity.x) / MaxSpeed);
        m_animator.SetFloat(m_YVelocityParamHashId, m_velocity.y);
        // TODO: Check if parameters needs delay
        m_animator.SetBool(IsGroundedParamNameString, IsGrounded);
        m_animator.SetBool(m_isSlidingOfWallParamHashId, IsSlidingOfWall/* && m_wasDashAnimeDelayed*/);
        m_animator.SetBool(m_isDashingParamHashId, IsDashing);
        m_animator.SetBool(m_knockedBackParamHashId, IsKnockedBack);

        if (m_triggeredAirborneJump)
        {
            m_animator.SetTrigger(m_airborneJumpParamHashId);
        }

        // MAYBE: 
        // Add a delay to play animations that need it:
        // Since the character is moved using the rigidbody2D position, the position of the character
        // will visually be the same only around when we reach the next FixedUpdate. Immediately updating
        // certain animations will actually make them happen too soon. Therefore, those animation needs to
        // wait until the next FixedUpdate.
        /*if (IsSlidingOfWall && !m_wasDashAnimeDelayed)
        {
            m_wasDashAnimeDelayed = true;
        }
        else if (!IsSlidingOfWall && m_wasDashAnimeDelayed)
        {
            m_wasDashAnimeDelayed = false;
        }*/
    }

    public void SetInputs(Inputs inputs)
    {
        if (!IsKnockedBack)
        {
            m_currentInputs = inputs;
        }
    }
    
    protected override void Update()
    {
        ComputeVelocity();
    }

    protected override void ComputeVelocity()
    {
        // Once a jump or a dash is triggered, neither can be triggered again until the next FixedUpdate is executed
        if (!m_triggeredJump && !m_triggeredAirborneJump && !m_triggeredDash)
        {
            // Jump
            if (m_canJump && IsGrounded && m_currentInputs.jump)
            {
                Jump();
            }
            // Wall jump
            else if (m_canWallJump && (IsSlidingOfWall || InWallJumpWindow()) && m_currentInputs.jump)
            {
                WallJump();
            }
            // Airborne jump
            else if (m_canAirborneJump && !IsGrounded && m_airborneJumpAvailable && m_currentInputs.jump)
            {
                AirborneJump();
            }
            // Dash
            else if (m_canDash && m_currentInputs.dash && !InDashWindow() && !DashInCooldown())
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
            m_targetHorizontalVelocity = m_currentInputs.horizontal * MaxSpeed;
        }
    }

    // Used to push away. Used in, for exemple, explosion 
    public void KnockBack(Vector2 knockBackVelocity, float knockBackDelay)
    {
        // Cancel any input that was queued
        m_currentInputs = new Inputs();

        // Reset variables
        m_triggeredJump = false;
        m_triggeredAirborneJump = false;
        m_triggeredDash = false;

        IsSlidingOfWall = false;
        IsDashing = false;

        // Cancel all possible coroutines
        if (InWallJumpWindow())
        {
            EndWallJumpWindow();
        }

        if (HorizontalControlDelayed())
        {
            EndDelayedHorizontalControl();
        }

        if (InDashWindow())
        {
            EndDashWindow();
        }

        // Set the movement
        m_targetHorizontalVelocity = knockBackVelocity.x;
        AddJumpImpulse(knockBackVelocity.y);

        IsKnockedBack = true;

        // Start delay for controls
        m_horizontalControlDelayCoroutine = DelayHorizontalControl(knockBackDelay);
        StartCoroutine(m_horizontalControlDelayCoroutine);
    }

    // Jump related methods
    private void Jump()
    {
        AddJumpImpulse(JumpTakeOffSpeed);
        m_triggeredJump = true;

        // Play sound
        m_audioSource.pitch = Random.Range(.9f, 1.0f);
        m_audioSource.PlayOneShot(m_jumpSound);
    }
    
    private void AirborneJump()
    {
        AddJumpImpulse(m_airborneJumpTakeOffSpeed);
        m_airborneJumpAvailable = false;
        m_triggeredAirborneJump = true;

        // Reset canceled jump to allow airborne jump to be canceled
        m_jumpCanceled = false;

        // Play sound
        m_audioSource.pitch = Random.Range(.9f, 1.0f);
        m_audioSource.PlayOneShot(m_airborneJumpSound);
    }

    private void AddJumpImpulse(float takeOffSpeed)
    {
        m_velocity.y = takeOffSpeed;

        // Reset ground normal because it is use for the movement along ground
        // If it's not reset, it could cause the player to move in a strange direction 
        //m_groundNormal = new Vector2(.0f, 1.0f);

        // Update the gravity modifier
        m_currentGravityModifier = m_gravityModifier;
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

        if (InWallJumpWindow())
        {
            EndWallJumpWindow();
        }

        m_horizontalControlDelayCoroutine = DelayHorizontalControl(m_WallJumpControlDelayTime);
        StartCoroutine(m_horizontalControlDelayCoroutine);
    }

    private IEnumerator DelayHorizontalControl(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_horizontalControlDelayCoroutine = null;

        if (IsKnockedBack == true)
        {
            ResetKnockbackVariables();
        }
    }

    private void EndDelayedHorizontalControl()
    {
        StopCoroutine(m_horizontalControlDelayCoroutine);

        if (IsKnockedBack == true)
        {
            ResetKnockbackVariables();
        }
    }
    
    private void ResetKnockbackVariables()
    {
        IsKnockedBack = false;

        // Reset the last target horizontal velocity direction to prevent the sprite from flipping to the wrong side during the next Animate() method call
        if (!IsSlidingOfWall)
        {
            m_lastHorizontalVelocityDirection = -Mathf.Sign(m_targetHorizontalVelocity);
        }

        // Reset the target horizontal velocity here, because the fixed update might be called first before the next update
        m_targetHorizontalVelocity = .0f;
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
        //m_groundNormal = new Vector2(.0f, 1.0f);

        // Update the gravity modifier
        m_currentGravityModifier = .0f;

        m_triggeredDash = true;

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

        // Tell subscribers that the dash was used
        foreach (IPlatformerMovementSubscriber subscriber in m_subscribers)
        {
            subscriber.NotifyDashUsed();
        }

        // Play sounds
        m_audioSource.pitch = Random.Range(.9f, 1.0f);
        m_audioSource.PlayOneShot(m_dashSound);
    }

    private IEnumerator UseDashWindow()
    {
        yield return new WaitForSeconds(m_dashDuration);
        EndDashWindow();
    }

    // Ending the dash window doesn't directly change the IsDashing flag, because it should be set during a FixedUpdate
    // and this could be called outside of one
    private void EndDashWindow()
    {
        StopCoroutine(m_dashWindowCoroutine);
        m_dashWindowCoroutine = null;

        // Update the gravity modifier
        m_currentGravityModifier = m_gravityModifier;
        
        m_dashCooldownCoroutine = DashCooldown();
        StartCoroutine(m_dashCooldownCoroutine);
    }

    // Return dashing state base on the coroutine existence, since using the actual movement could create incorrect values!
    // This could happen because moving the rigidbody doesn't immediatly update the transform and wall slide detection doesn't
    // use the rigidbody position
    private bool InDashWindow()
    {
        return m_dashWindowCoroutine != null;
    }

    private IEnumerator DashCooldown()
    {
        float elapsedTime = .0f;
        float lastUpdateTime = Time.time;

        while (elapsedTime < m_dashCooldown)
        {
            yield return 0;

            elapsedTime += (Time.time - lastUpdateTime); 

            // Tell subscribers that the dash cooldown was updated
            foreach (IPlatformerMovementSubscriber subscriber in m_subscribers)
            {
                subscriber.NotifyDashCooldownUpdated(Mathf.Clamp01(elapsedTime / m_dashCooldown));
            }
        }

        m_dashCooldownCoroutine = null;

        // Tell subscribers that the dash cooldown is over
        foreach (IPlatformerMovementSubscriber subscriber in m_subscribers)
        {
            subscriber.NotifyDashCooldownOver();
        }
    }

    private bool DashInCooldown()
    {
        return m_dashCooldownCoroutine != null;
    }

    public void EnableAirborneJump(bool enable)
    {
        m_canAirborneJump = enable;
    }

    public void EnableSlideOfWall(bool enable)
    {
        m_canSlideOfWall = enable;
    }

    public void EnableWallJump(bool enable)
    {
        m_canWallJump = enable;
    }

    public void EnableDash(bool enable)
    {
        m_canDash = enable;
    }
}
