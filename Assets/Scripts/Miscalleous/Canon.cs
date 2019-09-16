using UnityEngine;

public class Canon : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField]
    private Vector2 _initialAimingDirection = new Vector2(1.0f, 0.01f);
    [SerializeField]
    private float _canonRotationSpeed = 2.0f;
    [SerializeField]
    private float _rotationThreshold = 0.01f;

    private Vector2 _aimingDirection;

    [Header("Shoot")]
    [SerializeField]
    private GameObject _explosionEffect;

    [Header("Bullet")]
    [SerializeField]
    private Transform _bulletSpawnPosition;
    [SerializeField]
    private GameObject _bulletPrefab;

    private void Awake()
    {
        if (_rotationThreshold <= .0f)
        {
            Debug.LogError("The rotation threshold should be bigger than 0. If not, the canon could rotate in the wrong direction.");
        }

        if (!_bulletSpawnPosition)
        {
            Debug.LogError("No bullet spawn position was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_bulletPrefab)
        {
            Debug.LogError("No bullet prefab was set for " + GetType() + " script of " + gameObject.name + "!");
        }
    }

    private void Start()
    {
        _aimingDirection = _initialAimingDirection;

        float angle = Mathf.Atan2(_aimingDirection.y, _aimingDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(.0f, .0f, angle);
    }

    private void Update()
    {
        float angle = Mathf.Atan2(_aimingDirection.y, _aimingDirection.x) * Mathf.Rad2Deg;
        angle = Mathf.Clamp(angle, 0.01f, 179.99f);

        Quaternion currentRotation = Quaternion.Euler(transform.rotation.eulerAngles);
        Quaternion targetRotation = Quaternion.Euler(.0f, .0f, angle);

        // Rotate the canon over time
        transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, _canonRotationSpeed * Time.deltaTime);
    }

    public void SetAimingDirection(Vector2 direction)
    {
        _aimingDirection = new Vector2(direction.x, Mathf.Clamp01(direction.y)).normalized;
    }

    public GameObject Shoot()
    {
        // Show explosion effect
        Instantiate(_explosionEffect, _bulletSpawnPosition.position, Quaternion.identity);

        return Instantiate(_bulletPrefab, _bulletSpawnPosition.position, transform.rotation);
    }
}
