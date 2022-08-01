using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public class ChargeCannon : MonoBehaviour
{
    [Header("Charge")]
    [SerializeField]
    private float _minChargeTime = 0.5f;
    [SerializeField]
    private float _maxChargeTime = 3.0f;
    [SerializeField]
    private AudioClip _chargeStepSound;

    public bool IsCharging { get; private set; }

    private float _chargeDuration = .0f;

    private const int _chargeStepCount = 4;
    private float _chargeStep;
    private float _chargeSinceLastPoint = .0f;

    [Header("Projectile")]
    [SerializeField]
    private GameObject _projectilePrefab;
    [SerializeField]
    private Transform _projectileSpawnPoint;
    [SerializeField]
    private float _projectileMinVelocity = 500.0f;
    [SerializeField]
    private float _projectileMaxVelocity = 1000.0f;
    [SerializeField]
    private AudioClip _shootSound;

    private const string IsChargingParamNameString = "IsCharging";
    private const string ChargeParamNameString = "Charge";
    private const string PossessedModeAnimationLayerName = "Possessed Mode";

    protected int IsChargingParamHashId = Animator.StringToHash(IsChargingParamNameString);
    protected int ChargeParamHashId = Animator.StringToHash(ChargeParamNameString);
    private int PossessedModeAnimationLayerIndex;

    private Animator _animator;
    private AudioSource _audioSource;

    private void Awake()
    {
        _chargeStep = _maxChargeTime / _chargeStepCount;

        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        if (!_projectilePrefab)
        {
            Debug.LogError("No projectile prefab was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        PossessedModeAnimationLayerIndex = _animator.GetLayerIndex(PossessedModeAnimationLayerName);

        UpdatePossessedVisual(false);
    }

    private void Update()
    {
        if (IsCharging && _chargeDuration < _maxChargeTime)
        {
            _chargeDuration = Mathf.Clamp(_chargeDuration + Time.deltaTime, .0f, _maxChargeTime);
            _chargeSinceLastPoint += Time.deltaTime;

            if (_chargeSinceLastPoint >= _chargeStep)
            {
                _chargeSinceLastPoint -= _chargeStep;
                _audioSource.PlayOneShot(_chargeStepSound);
            }
        }

        Animate();
    }

    private void Animate()
    {
        _animator.SetBool(IsChargingParamHashId, IsCharging);
        _animator.SetFloat(ChargeParamHashId, _chargeDuration / _maxChargeTime);
    }

    public void UpdatePossessedVisual(bool possessed)
    {
        _animator.SetLayerWeight(PossessedModeAnimationLayerIndex, possessed ? 1.0f : .0f);
    }

    public void StartCharge()
    {
        IsCharging = true;
    }

    public void StopCharge()
    {
        IsCharging = false;

        if (!_projectilePrefab)
        {
            return;
        }

        if (_chargeDuration >= _minChargeTime)
        {
            GameObject projectile = Instantiate(_projectilePrefab, _projectileSpawnPoint.position, _projectileSpawnPoint.rotation);
            Rigidbody2D rigidbody = projectile.GetComponent<Rigidbody2D>();

            if (rigidbody)
            {
                float forceStrength = Mathf.Lerp(_projectileMinVelocity, _projectileMaxVelocity, (_chargeDuration - _minChargeTime) / (_maxChargeTime - _minChargeTime));
                rigidbody.AddForce(transform.right * forceStrength);
            }

            _audioSource.PlayOneShot(_shootSound);
        }

        _chargeDuration = .0f;
        _chargeSinceLastPoint = .0f;
    }
}
