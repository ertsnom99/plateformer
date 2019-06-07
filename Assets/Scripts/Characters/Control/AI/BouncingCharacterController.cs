using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]

public class BouncingCharacterController : PossessableCharacterController, IBouncingPhysicsObjectSubscriber, IBouncingFormControllerSubscriber
{
    // Collider that is use for collision during movement (it will be enable and disable when changing form)
    [SerializeField]
    private Collider2D _movementCollider;

    [Header("Charge")]
    [SerializeField]
    private float _maxChargeTime = 5.0f;
    private bool _isCharging = false;
    private float _chargeTime = .0f;

    // TEMP
    [SerializeField]
    private Text _chargeText;

    [Header("Bounce")]
    [SerializeField]
    private GameObject _arrow;
    [SerializeField]
    private GameObject _bounceFormPrefab;
    private GameObject _bounceForm;
    [SerializeField]
    private Collider2D _normalFormSpawnArea;
    [SerializeField]
    private Collider2D _bounceFormSpawnArea;
    protected ContactFilter2D ContactFilter;
    [SerializeField]
    private float _minLaunchStrength = 50.0f;
    [SerializeField]
    private float _maxLaunchStrength = 100.0f;
    [SerializeField]
    private CinemachineVirtualCamera _virtualCameraBounceForm;

    [Header("Sound")]
    [SerializeField]
    private AudioClip _chargeSound;
    [SerializeField]
    private AudioClip _launchSound;
    [SerializeField]
    private AudioClip _returnToNormalFormSound;
    
    private PlatformerMovement _movementScript;
    private BouncingFormCharacterController _bouncingFormController;
    private PossessableBouncingPhysicsObject _possessableBouncingPhysicsObject;

    protected override void Awake()
    {
        base.Awake();

        _arrow.SetActive(false);

        // Tells to use the layer settings from the Physics2D settings (the matrix)
        ContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        ContactFilter.useLayerMask = true;
        ContactFilter.useTriggers = false;

        // Get all component, correctly set them up and check that nothing is missing
        _movementScript = GetComponent<PlatformerMovement>();

        if (!_movementCollider)
        {
            Debug.LogError("No movement collider was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_bounceFormPrefab)
        {
            Debug.LogError("No bouncing form gameobject was set for " + GetType() + " script of " + gameObject.name + "!");
        }
        else
        {
            _bounceForm = Instantiate(_bounceFormPrefab);

            _bouncingFormController = _bounceForm.GetComponent<BouncingFormCharacterController>();
            _possessableBouncingPhysicsObject = _bounceForm.GetComponent<PossessableBouncingPhysicsObject>();


            if (!_bouncingFormController)
            {
                Debug.LogError("No BouncingFormController script on the bouncing form gameobject of " + gameObject.name + "!");
            }
            else
            {
                _bouncingFormController.Subscribe(this);
                _bouncingFormController.SetPossessionVirtualCamera(_virtualCameraBounceForm);
            }

            if (!_possessableBouncingPhysicsObject)
            {
                Debug.LogError("No PossessableBouncingPhysicsObject script on the bouncing form gameobject of " + gameObject.name + "!");
            }
            else
            {
                _possessableBouncingPhysicsObject.Subscribe(this);
            }

            if (!_virtualCameraBounceForm)
            {
                Debug.LogError("No virtual camera for bouncing form was set in " + GetType() + " script of " + gameObject.name + "!");
            }
            else
            {
                _virtualCameraBounceForm.Follow = _bounceForm.transform;
            }
        }

        if (!_normalFormSpawnArea)
        {
            Debug.LogError("No normal form spawn area was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_bounceFormSpawnArea)
        {
            Debug.LogError("No bounce form spawn area was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        // TEMP
        _chargeText.text = "";
    }

    protected override void Start()
    {
        base.Start();

        ShowNormalForm(false);
    }

    protected override void OnUpdatePossessed()
    {
        Inputs inputs = NoControlInputs;

        if (ControlsEnabled())
        {
            // Get the inputs used during this frame
            inputs = FetchInputs();
        }

        // Check if charge started
        if (_movementScript.IsGrounded && !_isCharging && inputs.HeldCharge/* && HasEnoughSpaceForBounceForm()*/)
        {
            StartCharge();
        }
        else if (_isCharging && inputs.ReleaseCharge)
        {
            StopCharge(inputs);
        }

        if (!_isCharging)
        {
            UpdateMovement(inputs);
        }
        else
        {
            // Update the charge arrow
            if (inputs.Vertical != .0f || inputs.Horizontal != .0f)
            {
                _arrow.SetActive(true);

                float angle = Mathf.Atan2(inputs.Vertical, inputs.Horizontal) * Mathf.Rad2Deg;
                _arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            else
            {
                _arrow.SetActive(false);
            }

            // Update charge time
            if (_chargeTime < _maxChargeTime)
            {
                _chargeTime = Mathf.Clamp(_chargeTime + Time.deltaTime, .0f, _maxChargeTime);

                // TEMP
                _chargeText.text = Mathf.Lerp(_minLaunchStrength, _maxLaunchStrength, _chargeTime / _maxChargeTime).ToString();
            }
        }

        UpdatePossession(inputs);
    }

    protected override Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();

        if (UseKeyboard)
        {
            // Inputs from the keyboard
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Jump = Input.GetButtonDown("Jump");
            inputs.ReleaseJump = Input.GetButtonUp("Jump");
            inputs.HeldCharge = Input.GetButton("Charge");
            inputs.ReleaseCharge = Input.GetButtonUp("Charge");
            inputs.Possess = Input.GetButtonDown("Possess");
        }
        else
        {
            // TODO: Create inputs specific to the controler
            // Inputs from the controler
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Jump = Input.GetButtonDown("Jump");
            inputs.ReleaseJump = Input.GetButtonUp("Jump");
            inputs.HeldCharge = Input.GetButton("Charge");
            inputs.ReleaseCharge = Input.GetButtonUp("Charge");
            inputs.Possess = Input.GetButtonDown("Possess");
        }

        return inputs;
    }

    protected override void OnUpdateNotPossessed()
    {
        base.OnUpdateNotPossessed();

        Inputs inputs = NoControlInputs;

        if (ControlsEnabled())
        {
            CreateInputs();
        }

        // Send the final inputs to the movement script
        UpdateMovement(inputs);
    }

    protected override Inputs CreateInputs()
    {
        Inputs inputs = NoControlInputs;

        //Vector3 positionToTargetWaypoint = Path.vectorPath[TargetWaypoint] - transform.position;

        return inputs;
    }

    public override bool Possess(Possession possessingScript)
    {
        if (IsPossessable && !IsPossessed)
        {
            PossessingScript = possessingScript;

            IsPossessed = true;

            Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, 1.0f);

            // When the character was taken possession of while it was actif
            if (SpriteRenderer.enabled)
            {
                VirtualCameraManager.Instance.ChangeVirtualCamera(PossessionVirtualCamera);

                AudioSource.pitch = Random.Range(.9f, 1.0f);
                AudioSource.PlayOneShot(OnPossessSound);

                _bouncingFormController.Possess(possessingScript);
            }
        }

        return IsPossessed;
    }

    public override bool Unpossess()
    {
        if (IsPossessed && IsPossessed)
        {
            IsPossessed = false;

            Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, .0f);

            if (SpriteRenderer.enabled)
            {
                if (PossessingScript)
                {
                    // Select the correct player spawn and respawn facing direction
                    Vector2 respawnPos;
                    Vector2 respawnFacingDirection;

                    if ((SpriteRenderer.flipX && !FlipPlayerSpawn) || (!SpriteRenderer.flipX && FlipPlayerSpawn))
                    {
                        respawnPos = LeftPlayerSpawn.transform.position;
                        respawnFacingDirection = Vector2.left;
                    }
                    else
                    {
                        respawnPos = RightPlayerSpawn.transform.position;
                        respawnFacingDirection = Vector2.right;
                    }

                    // Tell the possession script, that took possession of this AIController, that isn't in control anymore
                    PossessingScript.ReleasePossession(respawnPos, respawnFacingDirection);

                    PossessingScript = null;
                }

                AudioSource.pitch = Random.Range(.9f, 1.0f);
                AudioSource.PlayOneShot(OnUnpossessSound);

                _bouncingFormController.Unpossess();
            }
        }

        return IsPossessed;
    }

    /*void OnDrawGizmos()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 0, 0, 0.5f);

        Vector2 point = (Vector2)transform.position + m_collider.offset;
        Vector2 size = m_collider.size + new Vector2(2.0f, 2.0f) * m_collider.edgeRadius;

        Gizmos.DrawCube(point, size);
    }*/

    private bool HasEnoughSpaceForNormalForm()
    {
        // Check if there is a collider in the way
        Collider2D[] hitColliders = new Collider2D[8];
        _normalFormSpawnArea.OverlapCollider(ContactFilter, hitColliders);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider && collider.gameObject != _bounceForm && !collider.isTrigger)
            {
                return false;
            }
        }
        
        return true;
    }

    private void ShowNormalForm(bool replace)
    {
        // Hide the bouncing form
        _bounceForm.SetActive(false);
        
        // Replace and show the current character
        if (replace)
        {
            AlignNormalFormToBouncingForm();
        }

        SpriteRenderer.enabled = true;
        _movementCollider.enabled = true;
        _movementScript.enabled = true;

        EnableControl(true);

        if (IsPossessed)
        {
            // Update the virtual camera to use
            VirtualCameraManager.Instance.ChangeVirtualCamera(PossessionVirtualCamera);
        }
    }

    private void AlignNormalFormToBouncingForm()
    {
        transform.position = _bounceForm.transform.position - _bounceFormSpawnArea.transform.localPosition - (Vector3)_bounceFormSpawnArea.offset;
    }

    /*private bool HasEnoughSpaceForBounceForm()
    {
        //float overlapRadius = Mathf.Max(_bounceFormSpawnAreaCollider.transform.lossyScale.x, _bounceFormSpawnAreaCollider.transform.lossyScale.y) * _bounceFormSpawnAreaCollider.radius;
        //Vector2 overlapPoint = (Vector2)_bounceFormSpawnAreaCollider.transform.position + _bounceFormSpawnAreaCollider.offset;

        // Check if there is a collider in the way
        //Collider2D[] hitColliders = Physics2D.OverlapCircleAll(overlapPoint, overlapRadius);
        Collider2D[] hitColliders = new Collider2D[4];
        _bounceFormSpawnArea.OverlapCollider(ContactFilter, hitColliders);

        foreach (Collider2D collider in hitColliders)
        {
            if (collider && collider.gameObject != gameObject && !collider.isTrigger)
            {
                return false;
            }
        }

        return true;
    }*/

    private void ShowBouncingForm(bool replace)
    {
        // Hide the current character
        SpriteRenderer.enabled = false;
        _movementCollider.enabled = false;
        _movementScript.enabled = false;

        EnableControl(false);

        // Replace and show the bouncing form
        if (replace)
        {
            AlignBouncingFormToNormalForm();
        }

        _bounceForm.SetActive(true);

        if (IsPossessed)
        {
            // Update the virtual camera to use
            VirtualCameraManager.Instance.ChangeVirtualCamera(_virtualCameraBounceForm);
        }
    }

    private void AlignBouncingFormToNormalForm()
    {
        _bounceForm.transform.position = (Vector2)_bounceFormSpawnArea.transform.position + _bounceFormSpawnArea.offset;
    }

    private void StartCharge()
    {
        UpdateMovement(NoControlInputs);

        //_bounceFormSpawnAreaCollider.enabled = true;
        AudioSource.PlayOneShot(_chargeSound);

        _chargeTime = .0f;
        _isCharging = true;
    }

    private void StopCharge(Inputs inputs)
    {
        _arrow.SetActive(false);
        AudioSource.Stop();

        _isCharging = false;

        // Only launch of a launch direction is given
        if (inputs.Vertical != .0f || inputs.Horizontal != .0f)
        {
            ShowBouncingForm(true);
            Launch(inputs);
        }

        // TEMP
        _chargeText.text = "";
    }

    private void Launch(Inputs inputs)
    {
        // Launch bouncing form
        Vector2 launchDirection = new Vector2(inputs.Horizontal, inputs.Vertical).normalized;
        Vector2 launchForce = launchDirection * Mathf.Lerp(_minLaunchStrength, _maxLaunchStrength, _chargeTime / _maxChargeTime);
        _possessableBouncingPhysicsObject.Launch(launchForce);

        AudioSource.PlayOneShot(_launchSound);
    }

    private void UpdateMovement(Inputs inputs)
    {
        _movementScript.SetInputs(inputs);
    }

    protected override void UpdatePossession(Inputs inputs)
    {
        bool wasPossessed = IsPossessed;

        base.UpdatePossession(inputs);

        // Done here instead of in the "OnUnpossess" method, because we need to inputs for the "StopCharge" method
        if (_isCharging && wasPossessed && !IsPossessed)
        {
            StopCharge(inputs);
        }
    }

    private void ChangeToNormalForm()
    {
        // Replace the character to make sure that the correct area is tested
        AlignNormalFormToBouncingForm();

        if (HasEnoughSpaceForNormalForm())
        {
            ShowNormalForm(false);
            AudioSource.PlayOneShot(_returnToNormalFormSound);
        }
        else
        {
            StartCoroutine(WaitForEnoughSpaceForNormalForm());
        }
    }

    private IEnumerator WaitForEnoughSpaceForNormalForm()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            // Replace the character to make sure that the correct area is tested for collision
            AlignNormalFormToBouncingForm();

            if (HasEnoughSpaceForNormalForm())
            {
                ShowNormalForm(true);
                AudioSource.PlayOneShot(_returnToNormalFormSound);
                StopAllCoroutines();
            }
        }
    }

    public override bool ControlsEnabled()
    {
        return base.ControlsEnabled() && _possessableBouncingPhysicsObject.MovementFrozen;
    }

    public override void EnableControl(bool enable)
    {
        base.EnableControl(enable);

        if (!enable && _isCharging)
        {
            _arrow.SetActive(false);
            AudioSource.Stop();

            _isCharging = false;

            // TEMP
            _chargeText.text = "";
        }
    }

    // Methods of the IBouncingFormControllerSubscriber interface
    public void NotifyPossessed(Possession possessingScript)
    {
        Possess(possessingScript);
    }

    public void NotifyUnpossessed()
    {
        Unpossess();
    }

    public void NotifyCanceledBounce()
    {
        ChangeToNormalForm();
    }

    // Methods of the IBouncingPhysicsObjectSubscriber interface
    public void NotifyBounceStarted() { }

    public void NotifyBounceFinished()
    {
        ChangeToNormalForm();
    }
}
