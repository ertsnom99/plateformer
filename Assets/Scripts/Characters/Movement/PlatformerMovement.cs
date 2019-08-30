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
    private float _maxSpeed = 9.0f;

    public float MaxSpeed
    {
        get { return _maxSpeed; }
        private set { _maxSpeed = value; }
    }

    [Header("Jump")]
    [SerializeField]
    private bool _canJump = true;

    [SerializeField]
    private float _jumpTakeOffSpeed = 20.0f;

    public float JumpTakeOffSpeed
    {
        get { return _jumpTakeOffSpeed; }
        private set { _jumpTakeOffSpeed = value; }
    }

    private bool _triggeredJump = false;
    private bool _jumpCanceled = false;

    private float _lastHorizontalVelocityDirection;

    public bool IsKnockedBack { get; private set; }

    [Header("Airborne jump")]
    [SerializeField]
    private bool _canAirborneJump = true;
    [SerializeField]
    private float _airborneJumpTakeOffSpeed = 15.0f;
    private bool _airborneJumpAvailable = true;
    private bool _triggeredAirborneJump = false;

    // NOTE: The current implementation would cause problems if m_canSlideOfWall was changed during a wall slide
    [Header("Slide of wall")]
    [SerializeField]
    private bool _canSlideOfWall = true;
    [SerializeField]
    private LayerMask _wallLayers;
    [SerializeField]
    private bool _canSlideGoingUp = false;
    [SerializeField]
    private bool _canSlideDuringKnockBack = false;
    [SerializeField]
    private float _slideRaycastOffset = -.4f;
    [SerializeField]
    private float _slideRaycastDistance = .6f;
    [SerializeField]
    private float _slideGravityModifier = 1.0f;
    [SerializeField]
    private bool _constantSlipeSpeed = false;

    private bool _hitWall = false;

    public bool IsSlidingOfWall { get; private set; }

    [SerializeField]
    private bool _debugSlideRaycast = false;

    // NOTE: The current implementation needs wall slide to be enable for the wall jump to be used 
    [Header("Wall jump")]
    [SerializeField]
    private bool _canWallJump = true;
    [SerializeField]
    private float _wallJumpHorizontalVelocity = 6.0f;
    [SerializeField]
    private float _wallJumpWindowTime = .07f;
    private IEnumerator _wallJumpWindowCoroutine;
    [SerializeField]
    private float _WallJumpControlDelayTime = .1f;
    private IEnumerator _horizontalControlDelayCoroutine;

    [Header("Dash")]
    [SerializeField]
    private bool _canDash = true;
    [SerializeField]
    private float _dashSpeed = 26.0f;
    [SerializeField]
    private float _dashDuration = .2f;
    [SerializeField]
    private float _dashCooldown = 2.0f;
    private IEnumerator _dashWindowCoroutine;
    private IEnumerator _dashCooldownCoroutine;
    private bool _triggeredDash = false;

    public bool IsDashing { get; private set; }

    [Header("Sound")]
    [SerializeField]
    private AudioClip _jumpSound;
    [SerializeField]
    private AudioClip _airborneJumpSound;
    [SerializeField]
    private AudioClip _dashSound;

    private Inputs _emptyInputs = new Inputs();
    private Inputs _currentInputs;

    // MAYBE: Flags used to delay animations
    //private bool _wasDashAnimeDelayed = false;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private AudioSource _audioSource;

    protected int XVelocityParamHashId = Animator.StringToHash(XVelocityParamNameString);
    protected int YVelocityParamHashId = Animator.StringToHash(YVelocityParamNameString);
    protected int IsGroundedParamHashId = Animator.StringToHash(IsGroundedParamNameString);
    protected int IsSlidingOfWallParamHashId = Animator.StringToHash(IsSlidingOfWallParamNameString);
    protected int IsDashingParamHashId = Animator.StringToHash(IsDashingParamNameString);
    protected int AirborneJumpParamHashId = Animator.StringToHash(AirborneJumpParamNameString);
    protected int KnockedBackParamHashId = Animator.StringToHash(KnockedBackParamNameString);

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

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    protected override void FixedUpdate()
    {
        // Keep track of the position of the player before it's updated
        Vector2 previousRigidbodyPosition = Rigidbody2D.position;

        // Keep track of the direction the player intended to move in
        if (TargetHorizontalVelocity != .0f)
        {
            _lastHorizontalVelocityDirection = Mathf.Sign(TargetHorizontalVelocity);
        }

        // Reset the Y velocity if the player is suppose to keep the same velocity while sliding of a wall
        if (IsSlidingOfWall && _constantSlipeSpeed && Velocity.y < .0f)
        {
            Velocity = new Vector2(Velocity.x, .0f);
        }

        base.FixedUpdate();

        // Stop the dash window if an airborne jump was triggered during a dash
        if (InDashWindow() && _triggeredAirborneJump)
        {
            EndDashWindow();
        }

        // Update dashing state before UpdateWallSlide(), since it uses that flag
        IsDashing = InDashWindow();

        // Only update wall slide if it's allowed
        if (_canSlideOfWall)
        {
            UpdateWallSlide(Rigidbody2D.position - previousRigidbodyPosition);
        }

        // Cancel dash window if hitted a wall
        if (InDashWindow() && _hitWall)
        {
            EndDashWindow();
        }

        // Reset flags if they are in a certain state
        // While the airborne jump isn't available, keep it unavailable has long has the character isn't grounded and isn't sliding of a wall
        if (!_airborneJumpAvailable)
        {
            _airborneJumpAvailable = IsGrounded || IsSlidingOfWall;
        }

        // While the jump was canceled, keep it canceled has long has the character isn't grounded and isn't sliding of a wall
        if (_jumpCanceled)
        {
            _jumpCanceled = !IsGrounded && !IsSlidingOfWall;
        }

        // End the horizontal control delay if there is one, except for the knock back, and the player is grounded or sliding of a wall
        if (!IsKnockedBack && HorizontalControlDelayed() && (IsGrounded || IsSlidingOfWall))
        {
            EndDelayedHorizontalControl();
        }

        // Animate before resetting flags, since some of those flags are necessary for Animate()
        Animate();

        // Reset m_hitWall flag for next check
        _hitWall = false;

        // Reset flags used in Update
        _triggeredJump = false;
        _triggeredAirborneJump = false;
        _triggeredDash = false;

        DebugMovement();
    }

    private void DebugMovement()
    {
        if (_debugSlideRaycast)
        {
            Vector2 slideRaycastStart = new Vector2(transform.position.x, transform.position.y + _slideRaycastOffset);
            float lineDirection;

            if (_lastHorizontalVelocityDirection == .0f)
            {
                lineDirection = _spriteRenderer.flipX ? -1 : 1;
            }
            else
            {
                lineDirection = _lastHorizontalVelocityDirection;
            }

            Debug.DrawLine(slideRaycastStart, slideRaycastStart + lineDirection * Vector2.right * _slideRaycastDistance, Color.yellow);
        }
    }

    protected override void OnColliderHitCheck(RaycastHit2D hit)
    {
        // Set flag to tell that the character hit a wall
        if (IsLayerInWallLayers(hit.collider.gameObject.layer) && hit.normal.x != .0f && hit.normal.y == .0f)
        {
            _hitWall = true;

            // Reset the target velocity when the character is knocked back because it is suppose to loose it's velocity when that happens
            if (IsKnockedBack)
            {
                TargetHorizontalVelocity = .0f;
            }
        }
    }

    private bool IsLayerInWallLayers(int layer)
    {
        // See C# - Bitwise Operators (https://www.tutorialspoint.com/csharp/csharp_bitwise_operators.htm)
        // From Unity forum (https://forum.unity.com/threads/howto-if-layermask-contains-layer.30162/):
        /*What this does is shift the bit that you want to test into the rightmost bit (ie, the digit that
         * represents the value of 1). The bitwise-and operator then sets all bits except this one to zero.
         * The value in the integer is then the value of the specific bit you want to test - it will be zero
         * if false and one if true.*/
        return ((_wallLayers >> layer) & 1) == 1;
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
        else if ((_canSlideDuringKnockBack || !IsKnockedBack) && (_canSlideGoingUp || Velocity.y <= .0f) && _hitWall && !IsSlidingOfWall && !IsGrounded)
        {
            // Check if the player can slide of the wall
            IsSlidingOfWall = RaycastForWallSlide();
        }
        // Might need to stop sliding of a wall
        else if (IsSlidingOfWall)
        {
            // If the player moves away from wall
            if (!_hitWall && movementDirection.x != .0f)
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
                    _lastHorizontalVelocityDirection = -_lastHorizontalVelocityDirection;
                }
            }
        }

        // If the player started to slide of a new wall
        if (!wasSliding && IsSlidingOfWall)
        {
            Velocity = Vector2.zero;

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
        else if (wasSliding && !IsSlidingOfWall && !_triggeredJump && !IsDashing)
        {
            _wallJumpWindowCoroutine = WallJumpWindow();
            StartCoroutine(_wallJumpWindowCoroutine);
        }

        // Update the gravity modifier when the player change his sliding state and not because he dashed
        if (wasSliding != IsSlidingOfWall && !IsDashing)
        {
            CurrentGravityModifier = IsSlidingOfWall ? _slideGravityModifier : GravityModifier;
        }
    }

    private bool RaycastForWallSlide()
    {
        Vector2 slideRaycastStart = new Vector2(Rigidbody2D.position.x, Rigidbody2D.transform.position.y + _slideRaycastOffset);

        RaycastHit2D[] results = new RaycastHit2D[1];
        Physics2D.Raycast(slideRaycastStart, _lastHorizontalVelocityDirection * Vector2.right, ContactFilter, results, _slideRaycastDistance);

        return results[0].collider && IsLayerInWallLayers(results[0].collider.gameObject.layer);
    }

    private void Animate()
    {
        AdjustSpriteOrientation();

        // Update animator parameters
        _animator.SetFloat(XVelocityParamHashId, Mathf.Abs(Velocity.x) / MaxSpeed);
        _animator.SetFloat(YVelocityParamHashId, Velocity.y);
        _animator.SetBool(IsGroundedParamNameString, IsGrounded);
        _animator.SetBool(IsSlidingOfWallParamHashId, IsSlidingOfWall/* && m_wasDashAnimeDelayed*/);
        _animator.SetBool(IsDashingParamHashId, IsDashing);
        _animator.SetBool(KnockedBackParamHashId, IsKnockedBack);

        if (_triggeredAirborneJump)
        {
            _animator.SetTrigger(AirborneJumpParamHashId);
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

    private void AdjustSpriteOrientation()
    {
        // Flip the sprite if necessary
        bool flipSprite = false;

        if (!IsSlidingOfWall && !HorizontalControlDelayed())
        {
            flipSprite = (_spriteRenderer.flipX == _lastHorizontalVelocityDirection >= .0f);
        }
        else if (IsSlidingOfWall || IsKnockedBack)
        {
            flipSprite = (_spriteRenderer.flipX == _lastHorizontalVelocityDirection < .0f);
        }

        if (flipSprite)
        {
            _spriteRenderer.flipX = !_spriteRenderer.flipX;
        }
    }

    public void SetInputs(Inputs inputs)
    {
        if (!IsKnockedBack)
        {
            _currentInputs = inputs;
        }
    }

    protected override void Update()
    {
        ComputeVelocity();
    }

    protected override void ComputeVelocity()
    {
        // Once a jump or a dash is triggered, neither can be triggered again until the next FixedUpdate is executed
        if (!_triggeredJump && !_triggeredAirborneJump && !_triggeredDash)
        {
            // Jump
            if (_canJump && IsGrounded && _currentInputs.Jump)
            {
                Jump();
            }
            // Wall jump
            else if (_canWallJump && (IsSlidingOfWall || InWallJumpWindow()) && _currentInputs.Jump)
            {
                WallJump();
            }
            // Airborne jump
            else if (_canAirborneJump && !IsGrounded && _airborneJumpAvailable && _currentInputs.Jump)
            {
                AirborneJump();
            }
            // Dash
            else if (_canDash && _currentInputs.Dash && !InDashWindow() && !DashInCooldown())
            {
                Dash();
            }
        }

        // Cancel jump if release jump button while having some jump velocity remaining
        if (!_jumpCanceled && Velocity.y > .0f && _currentInputs.ReleaseJump)
        {
            CancelJump();
        }

        // Set the wanted horizontal velocity, except during the delayed controls window and the dash window
        if (!HorizontalControlDelayed() && !InDashWindow())
        {
            TargetHorizontalVelocity = _currentInputs.Horizontal * MaxSpeed;
        }
    }

    public void ChangeFacingDirection(Vector2 facingDirection)
    {
        _lastHorizontalVelocityDirection = facingDirection.x;

        AdjustSpriteOrientation();
    }

    // Used to push away. Used in, for exemple, explosion 
    public void KnockBack(Vector2 knockBackVelocity, float knockBackDelay)
    {
        // Cancel any input that was queued
        _currentInputs = new Inputs();

        // Reset variables
        _triggeredJump = false;
        _triggeredAirborneJump = false;
        _triggeredDash = false;

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
        TargetHorizontalVelocity = knockBackVelocity.x;
        
        AddJumpImpulse(knockBackVelocity.y);

        IsKnockedBack = true;
        
        // Start delay for controls
        _horizontalControlDelayCoroutine = DelayHorizontalControl(knockBackDelay);
        StartCoroutine(_horizontalControlDelayCoroutine);
    }

    public void EndKnockBack()
    {
        EndDelayedHorizontalControl();
    }

    // Jump related methods
    private void Jump()
    {
        AddJumpImpulse(JumpTakeOffSpeed);
        _triggeredJump = true;

        // Play sound
        _audioSource.pitch = Random.Range(.9f, 1.0f);
        _audioSource.PlayOneShot(_jumpSound);
    }
    
    private void AirborneJump()
    {
        AddJumpImpulse(_airborneJumpTakeOffSpeed);
        _airborneJumpAvailable = false;
        _triggeredAirborneJump = true;

        // Reset canceled jump to allow airborne jump to be canceled
        _jumpCanceled = false;

        // Play sound
        _audioSource.pitch = Random.Range(.9f, 1.0f);
        _audioSource.PlayOneShot(_airborneJumpSound);
    }

    private void AddJumpImpulse(float takeOffSpeed)
    {
        Velocity = new Vector2(Velocity.x, takeOffSpeed);

        // Update the gravity modifier
        CurrentGravityModifier = GravityModifier;
    }

    private IEnumerator WallJumpWindow()
    {
        yield return new WaitForSeconds(_wallJumpWindowTime);
        EndWallJumpWindow();
    }

    private void EndWallJumpWindow()
    {
        if (_wallJumpWindowCoroutine != null)
        {
            StopCoroutine(_wallJumpWindowCoroutine);
            _wallJumpWindowCoroutine = null;
        }
    }
    
    private bool InWallJumpWindow()
    {
        return _wallJumpWindowCoroutine != null;
    }

    private void WallJump()
    {
        Jump();
        TargetHorizontalVelocity = _spriteRenderer.flipX ? -_wallJumpHorizontalVelocity : _wallJumpHorizontalVelocity;

        if (InWallJumpWindow())
        {
            EndWallJumpWindow();
        }

        _horizontalControlDelayCoroutine = DelayHorizontalControl(_WallJumpControlDelayTime);
        StartCoroutine(_horizontalControlDelayCoroutine);
    }

    private IEnumerator DelayHorizontalControl(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndDelayedHorizontalControl();
    }

    private void EndDelayedHorizontalControl()
    {
        if (_horizontalControlDelayCoroutine != null)
        {
            StopCoroutine(_horizontalControlDelayCoroutine);
            _horizontalControlDelayCoroutine = null;

            if (IsKnockedBack == true)
            {
                ResetKnockbackVariables();
            }
        }
    }
    
    private void ResetKnockbackVariables()
    {
        IsKnockedBack = false;

        // Reset the last target horizontal velocity direction to prevent the sprite from flipping to the wrong side during the next Animate() method call
        if (!IsSlidingOfWall)
        {
            _lastHorizontalVelocityDirection = -Mathf.Sign(TargetHorizontalVelocity);
        }

        // Reset the target horizontal velocity here, because the fixed update might be called first before the next update
        TargetHorizontalVelocity = .0f;
    }

    private bool HorizontalControlDelayed()
    {
        return _horizontalControlDelayCoroutine != null;
    }

    private void CancelJump()
    {
        Velocity = new Vector2(Velocity.x, Velocity.y * 0.5f);
        _jumpCanceled = true;
    }

    // Dash related methods
    private void Dash()
    {
        Velocity = new Vector2(Velocity.x, .0f);
        TargetHorizontalVelocity = _spriteRenderer.flipX ? -_dashSpeed : _dashSpeed;

        // Update the gravity modifier
        CurrentGravityModifier = .0f;

        _triggeredDash = true;

        // Cancel the other coroutines if necessary
        if (InWallJumpWindow())
        {
            EndWallJumpWindow();
        }

        if (HorizontalControlDelayed())
        {
            EndDelayedHorizontalControl();
        }

        _dashWindowCoroutine = UseDashWindow();
        StartCoroutine(_dashWindowCoroutine);

        // Tell subscribers that the dash was used
        foreach (IPlatformerMovementSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyDashUsed();
        }

        // Play sounds
        _audioSource.pitch = Random.Range(.9f, 1.0f);
        _audioSource.PlayOneShot(_dashSound);
    }

    private IEnumerator UseDashWindow()
    {
        yield return new WaitForSeconds(_dashDuration);
        EndDashWindow();
    }

    // Ending the dash window doesn't directly change the IsDashing flag, because it should be set during a FixedUpdate
    // and this could be called outside of one
    private void EndDashWindow()
    {
        if (_dashWindowCoroutine != null)
        {
            StopCoroutine(_dashWindowCoroutine);
            _dashWindowCoroutine = null;

            // Update the gravity modifier
            CurrentGravityModifier = GravityModifier;

            if (gameObject.activeSelf)
            {
                _dashCooldownCoroutine = DashCooldown();
                StartCoroutine(_dashCooldownCoroutine);
            }
        }
    }

    // Return dashing state base on the coroutine existence, since using the actual movement could create incorrect values!
    // This could happen because moving the rigidbody doesn't immediatly update the transform and wall slide detection doesn't
    // use the rigidbody position
    private bool InDashWindow()
    {
        return _dashWindowCoroutine != null;
    }

    private IEnumerator DashCooldown()
    {
        float elapsedTime = .0f;
        float lastUpdateTime = Time.time;

        while (elapsedTime < _dashCooldown)
        {
            yield return 0;

            elapsedTime += (Time.time - lastUpdateTime); 

            // Tell subscribers that the dash cooldown was updated
            foreach (IPlatformerMovementSubscriber subscriber in Subscribers)
            {
                subscriber.NotifyDashCooldownUpdated(Mathf.Clamp01(elapsedTime / _dashCooldown));
            }
        }

        _dashCooldownCoroutine = null;

        // Tell subscribers that the dash cooldown is over
        foreach (IPlatformerMovementSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyDashCooldownOver();
        }
    }

    private void EndDashCooldown()
    {
        if (_dashCooldownCoroutine != null)
        {
            StopCoroutine(_dashCooldownCoroutine);
            _dashCooldownCoroutine = null;

            // Tell subscribers that the dash cooldown is over
            foreach (IPlatformerMovementSubscriber subscriber in Subscribers)
            {
                subscriber.NotifyDashCooldownUpdated(1.0f);
                subscriber.NotifyDashCooldownOver();
            }
        }
    }

    private bool DashInCooldown()
    {
        return _dashCooldownCoroutine != null;
    }

    public void EnableAirborneJump(bool enable)
    {
        _canAirborneJump = enable;
    }

    public void EnableSlideOfWall(bool enable)
    {
        _canSlideOfWall = enable;
    }

    public void EnableWallJump(bool enable)
    {
        _canWallJump = enable;
    }

    public void EnableDash(bool enable)
    {
        _canDash = enable;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        SetInputs(_emptyInputs);

        EndWallJumpWindow();
        EndDelayedHorizontalControl();
        EndDashWindow();
        EndDashCooldown();

        CurrentGravityModifier = GravityModifier;
        IsSlidingOfWall = false;
        IsDashing = false;
        IsKnockedBack = false;

        _airborneJumpAvailable = true;
        _jumpCanceled = false;
        _hitWall = false;
        _triggeredJump = false;
        _triggeredAirborneJump = false;
        _triggeredDash = false;
        _lastHorizontalVelocityDirection = .0f;
    }
}
