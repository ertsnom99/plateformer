using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]

public class BouncingCharacterController : AIController, IBouncingPhysicsObjectSubscriber
{
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

    [Header("Sound")]
    [SerializeField]
    private AudioClip _chargeSound;
    [SerializeField]
    private AudioClip _launchSound;
    [SerializeField]
    private AudioClip _returnToNormalFormSound;

    [Header("Camera")]
    [SerializeField]
    private CinemachineVirtualCamera _virtualCameraNormalForm;
    [SerializeField]
    private CinemachineVirtualCamera _virtualCameraBounceForm;
    
    private PlatformerMovement _movementScript;
    private BouncingPhysicsObject _bouncingPhysicsObjectScript;

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

        if (!_bounceFormPrefab)
        {
            Debug.LogError("No bouncing form gameobject was set!");
        }
        else
        {
            _bounceForm = Instantiate(_bounceFormPrefab);
            _bouncingPhysicsObjectScript = _bounceForm.GetComponent<BouncingPhysicsObject>();

            if (!_bouncingPhysicsObjectScript)
            {
                Debug.LogError("No BouncingPhysicsObject script on the bouncing form gameobject!");
            }
            else
            {
                _bouncingPhysicsObjectScript.Subscribe(this);
            }

            if (!_virtualCameraBounceForm)
            {
                Debug.LogError("No virtual camera for during bounce was set!");
            }
            else
            {
                _virtualCameraBounceForm.Follow = _bounceForm.transform;
            }
        }

        if (!_virtualCameraNormalForm)
        {
            Debug.LogError("No virtual camera for out of bounce was set!");
        }
        else
        {
            _virtualCameraNormalForm.Follow = transform;
        }

        if (!_normalFormSpawnArea)
        {
            Debug.LogError("No normal form spawn area was set!");
        }

        if (!_bounceFormSpawnArea)
        {
            Debug.LogError("No bounce form spawn area was set!");
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
        if (ControlsEnabled())
        {
            // Get the inputs used during this frame
            Inputs inputs = FetchInputs();

            // Check if charge started
            if (_movementScript.IsGrounded && !_isCharging && inputs.HeldCharge && HasEnoughSpaceForBounceForm())
            {
                UpdateMovement(NoControlInputs);

                //_bounceFormSpawnAreaCollider.enabled = true;
                AudioSource.PlayOneShot(_chargeSound);

                _chargeTime = .0f;
                _isCharging = true;
            }
            else if (_isCharging && inputs.ReleaseCharge)
            {
                //_bounceFormSpawnAreaCollider.enabled = false;
                _arrow.SetActive(false);
                AudioSource.Stop();

                _isCharging = false;

                // Only launch of a launch direction is given
                if (inputs.Vertical != .0f || inputs.Horizontal != .0f)
                {
                    ShowBouncingForm(true);
                    LaunchBouncingForm(inputs);
                }

                // TEMP
                _chargeText.text = "";
            }

            if (!_isCharging)
            {
                UpdateMovement(inputs);
            }
            else
            {
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

                if (_chargeTime < _maxChargeTime)
                {
                    _chargeTime = Mathf.Clamp(_chargeTime + Time.deltaTime, .0f, _maxChargeTime);

                    // TEMP
                    _chargeText.text = Mathf.Lerp(_minLaunchStrength, _maxLaunchStrength, _chargeTime / _maxChargeTime).ToString();
                }
            }
        }
    }

    protected override void OnUpdateNotPossessed()
    {
        base.OnUpdateNotPossessed();

        Inputs inputs = NoControlInputs;

        if (ControlsEnabled() && HasDetectedTarget && Path != null)
        {
            // TODO: Implement AI to create the correct inputs
        }

        // Send the final inputs to the movement script
        UpdateMovement(inputs);
    }

    /*void OnDrawGizmos()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 0, 0, 0.5f);

        Vector2 point = (Vector2)transform.position + m_collider.offset;
        Vector2 size = m_collider.size + new Vector2(2.0f, 2.0f) * m_collider.edgeRadius;

        Gizmos.DrawCube(point, size);
    }*/

    protected override bool ControlsEnabled()
    {
        return base.ControlsEnabled() && _bouncingPhysicsObjectScript.MovementFrozen;
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

    protected override Inputs CreateInputs()
    {
        Inputs inputs = NoControlInputs;

        //Vector3 positionToTargetWaypoint = Path.vectorPath[TargetWaypoint] - transform.position;
        
        return inputs;
    }

    private bool HasEnoughSpaceForNormalForm()
    {
        //Vector2 point = (Vector2)transform.position + _collider.offset;
        //Vector2 size = _collider.size + new Vector2(2.0f, 2.0f) * _collider.edgeRadius;

        // Check if there is a collider in the way
        //Collider2D[] colliders = Physics2D.OverlapBoxAll(point, size, .0f);
        Collider2D[] hitColliders = new Collider2D[4];
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
        _normalFormSpawnArea.enabled = true;
        _movementScript.enabled = true;

        if (IsPossessed)
        {
            // Update the virtual camera to use
            VirtualCameraManager.Instance.ChangeVirtualCamera(_virtualCameraNormalForm);
        }
    }

    private void AlignNormalFormToBouncingForm()
    {
        transform.position = _bounceForm.transform.position - _bounceFormSpawnArea.transform.localPosition - (Vector3)_bounceFormSpawnArea.offset;
    }

    private bool HasEnoughSpaceForBounceForm()
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
    }

    private void ShowBouncingForm(bool replace)
    {
        // Hide the current character
        SpriteRenderer.enabled = false;
        _normalFormSpawnArea.enabled = false;
        _movementScript.enabled = false;

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

    private void LaunchBouncingForm(Inputs inputs)
    {
        if (!_bounceForm.activeSelf)
        {
            Debug.LogError("Can't launch the bouncing form because it isn't active!!!");
        }
        else
        {
            Vector2 launchDirection = new Vector2(inputs.Horizontal, inputs.Vertical).normalized;
            Vector2 launchForce = launchDirection * Mathf.Lerp(_minLaunchStrength, _maxLaunchStrength, _chargeTime / _maxChargeTime);
            _bouncingPhysicsObjectScript.Launch(launchForce);

            AudioSource.PlayOneShot(_launchSound);
        }
    }

    protected override void UpdateMovement(Inputs inputs)
    {
        _movementScript.SetInputs(inputs);
    }

    protected override void UpdatePossession(Inputs inputs) { }

    // Methods of the IBouncingPhysicsObjectSubscriber interface
    public void NotifyBounceStarted() { }

    public void NotifyBounceFinished()
    {
        // Replace the character to make sure that the correct area is tested
        AlignNormalFormToBouncingForm();
        
        if (HasEnoughSpaceForNormalForm())
        {
            ShowNormalForm(true);
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
            // Replace the character to make sure that the correct area is tested for collision
            AlignNormalFormToBouncingForm();

            if (HasEnoughSpaceForNormalForm())
            {
                ShowNormalForm(true);
                AudioSource.PlayOneShot(_returnToNormalFormSound);
                StopAllCoroutines();
            }

            yield return null;
        }
    }
}
