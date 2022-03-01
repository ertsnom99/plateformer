using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]

public class BouncingCharacter : PossessablePawn, IBouncingPhysicsObjectSubscriber, IBouncingFormSubscriber
{
    [Header("Charge")]
    [SerializeField]
    private float _maxChargeTime = 5.0f;
    [SerializeField]
    private Slider _chargeBar;
    [SerializeField]
    private Image _fillBar;
    [SerializeField]
    private Color _minChargeColor;
    [SerializeField]
    private Color _maxChargeColor;
    private bool _isCharging = false;
    private float _chargeTime = .0f;

    [Header("Bounce")]
    [SerializeField]
    private GameObject _arrow;
    [SerializeField]
    private GameObject _bounceFormPrefab;
    [SerializeField]
    private Collider2D _normalFormSpawnArea;
    [SerializeField]
    private Collider2D _bounceFormSpawnArea;
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
    private BouncingFormCharacter _bouncingForm;
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
#if UNITY_EDITOR
        if (!_chargeBar)
        {
            Debug.LogError("No charger slider was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_fillBar)
        {
            Debug.LogError("No charger slider fill was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_bounceFormPrefab)
        {
            Debug.LogError("No bouncing form gameobject was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_virtualCameraBounceForm)
        {
            Debug.LogError("No virtual camera for bouncing form was set in " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_normalFormSpawnArea)
        {
            Debug.LogError("No normal form spawn area was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_bounceFormSpawnArea)
        {
            Debug.LogError("No bounce form spawn area was set for " + GetType() + " script of " + gameObject.name + "!");
        }
#endif
        _chargeBar.value = .0f;
        _chargeBar.gameObject.SetActive(false);

        _fillBar.color = _minChargeColor;

        _bouncingForm = Instantiate(_bounceFormPrefab).GetComponent<BouncingFormCharacter>();
        _possessableBouncingPhysicsObject = _bouncingForm.gameObject.GetComponent<PossessableBouncingPhysicsObject>();
        _bouncingForm.SetPossessionVirtualCamera(_virtualCameraBounceForm);
        _bouncingForm.Subscribe(this);
        _possessableBouncingPhysicsObject.Subscribe(this);
        _virtualCameraBounceForm.Follow = _bouncingForm.gameObject.transform;
#if UNITY_EDITOR
        if (!_bouncingForm)
        {
            Debug.LogError("No BouncingFormCharacter script on the bouncing form gameobject of " + gameObject.name + "!");
        }

        if (!_possessableBouncingPhysicsObject)
        {
            Debug.LogError("No PossessableBouncingPhysicsObject script on the bouncing form gameobject of " + gameObject.name + "!");
        }
#endif
    }

    protected override void Start()
    {
        base.Start();
        _bouncingForm.gameObject.SetActive(false);
    }

    public override void UpdateWithInputs(Inputs inputs)
    {
        // Check if charge started
        if (_movementScript.IsGrounded && !_isCharging && inputs.HeldPower && HasEnoughSpaceForBounceForm())
        {
            StartCharge();
        }
        else if (_isCharging && inputs.ReleasePower)
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
                _chargeBar.value = _chargeTime / _maxChargeTime;

                _fillBar.color = Color.Lerp(_minChargeColor, _maxChargeColor, _chargeTime / _maxChargeTime);
            }
        }

        base.UpdateWithInputs(inputs);
    }

    private void UpdateMovement(Inputs inputs)
    {
        _movementScript.SetInputs(inputs);
        _movementScript.UpdateMovement();
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

    private void ShowCharacter(bool show)
    {
        SpriteRenderer.enabled = show;
        Collider.enabled = show;
        _movementScript.enabled = show;
    }

    private bool HasEnoughSpaceForBounceForm()
    {
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

    private void ShowBouncingForm(bool replace = false)
    {
        Controller tempController = Controller;

        if (IsPossessed)
        {
            _bouncingForm.Possess(tempController, UnpossessBounds, ContactFilter.layerMask, OnUnpossessCallback);
        }
        else if (tempController)
        {
            tempController.SetControlledPawn(_bouncingForm);
        }

        ShowCharacter(false);

        // Replace and show the bouncing form
        if (replace)
        {
            AlignBouncingFormToNormalForm();
        }

        _bouncingForm.gameObject.SetActive(true);
    }

    private void AlignBouncingFormToNormalForm()
    {
        _bouncingForm.gameObject.transform.position = (Vector2)_bounceFormSpawnArea.transform.position + _bounceFormSpawnArea.offset;
    }

    private bool HasEnoughSpaceForNormalForm()
    {
        // Check if there is a collider in the way
        Collider2D[] overlapResults = new Collider2D[8];
        _normalFormSpawnArea.OverlapCollider(ContactFilter, overlapResults);

        foreach (Collider2D collider in overlapResults)
        {
            if (collider && collider.gameObject != _bouncingForm.gameObject && !collider.isTrigger)
            {
                return false;
            }
        }

        return true;
    }

    private void ShowNormalForm(bool replace = false)
    {
        Controller tempController = _bouncingForm.Controller;

        if (_bouncingForm.IsPossessed)
        {
            _bouncingForm.UnpossessWithoutSpawn();

            // Update the virtual camera to use
            VirtualCameraManager.Instance.ChangeVirtualCamera(PossessionVirtualCamera);
        }
        // Lost possession during bounce
        else if (IsPossessed)
        {
            IsPossessed = false;
            OnUnpossessCallback = null;
        }

        // Take bake the controller
        if (tempController)
        {
            tempController.SetControlledPawn(this);
        }

        // Hide the bouncing form
        _bouncingForm.gameObject.SetActive(false);

        // Replace and show the current character
        if (replace)
        {
            AlignNormalFormToBouncingForm();
        }

        ShowCharacter(true);
        UpdateVisual();
    }

    private void AlignNormalFormToBouncingForm()
    {
        transform.position = _bouncingForm.gameObject.transform.position - _bounceFormSpawnArea.transform.localPosition - (Vector3)_bounceFormSpawnArea.offset;
    }

    private void StartCharge()
    {
        UpdateMovement(NoControlInputs);

        AudioSource.PlayOneShot(_chargeSound);

        _isCharging = true;
        _chargeTime = .0f;
        _chargeBar.gameObject.SetActive(true);
    }

    private void StopCharge(Inputs inputs)
    {
        _arrow.SetActive(false);
        AudioSource.Stop();

        _isCharging = false;
        _chargeBar.value = .0f;
        _chargeBar.gameObject.SetActive(false);

        _fillBar.color = _minChargeColor;

        // Only launch of a launch direction is given
        if (inputs.Vertical != .0f || inputs.Horizontal != .0f)
        {
            ShowBouncingForm(true);
            Launch(inputs);
        }
    }

    private void Launch(Inputs inputs)
    {
        // Launch bouncing form
        Vector2 launchDirection = new Vector2(inputs.Horizontal, inputs.Vertical).normalized;
        Vector2 launchForce = launchDirection * Mathf.Lerp(_minLaunchStrength, _maxLaunchStrength, _chargeTime / _maxChargeTime);
        _possessableBouncingPhysicsObject.Launch(launchForce);

        AudioSource.PlayOneShot(_launchSound);
    }

    private void ChangeToNormalForm()
    {
        // Replace the character to make sure that the correct area is tested
        AlignNormalFormToBouncingForm();

        if (HasEnoughSpaceForNormalForm())
        {
            ShowNormalForm();
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

    // Methods of the IBouncingFormSubscriber interface
    public void NotifyPossessed(Controller controller, Bounds unpossessBounds, LayerMask unpossessCollisionMask, PossessablePawn.UnpossessCallbackDelegate onUnpossessCallback)
    {
        UnpossessBounds = unpossessBounds;
        ContactFilter.layerMask = unpossessCollisionMask;
        this.OnUnpossessCallback = onUnpossessCallback;
    }

    public void NotifyUnpossessed() { }

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
