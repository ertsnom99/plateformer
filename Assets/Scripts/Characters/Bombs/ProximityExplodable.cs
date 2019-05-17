using UnityEngine;

public interface IProximityExplodableSubscriber
{
    void NotifyCountdownStarted(GameObject explodableGameObject, float timeRemaining);
    void NotifyCountdownUpdated(GameObject explodableGameObject, float timeRemaining);
    void NotifyCountdownFinished(GameObject explodableGameObject);
}

public class ProximityExplodable : MonoSubscribable<IProximityExplodableSubscriber>
{
    [Header("Countdown")]
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private float _distanceToCountdown = 6.0f;
    [SerializeField]
    private Vector3 _distanceOffset = Vector3.zero;
    [SerializeField]
    private float _countdownTime = 3.0f;

    private bool _countdownStarted = false;
    private float _timeRemaining;

    [Header("Damage")]
    [SerializeField]
    private LayerMask _damagedLayers;
    [SerializeField]
    private float _explosionRange = 1.7f;
    [SerializeField]
    private int _damage = 10;

    [Header("Explosion")]
    [SerializeField]
    private GameObject _explosionEffect;
    [SerializeField]
    private Transform _explosionPosition;

    [Header("Knock Back")]
    [SerializeField]
    private Vector3 _knockBackDirection = new Vector3 (1.0f, 1.0f, .0f);
    [SerializeField]
    private float _knockBackStrength = 12.0f;
    [SerializeField]
    private float _knockBackDuration = .3f;

    [Header("Debug")]
    [SerializeField]
    private bool _drawDistanceToCountdown = false;
    [SerializeField]
    private bool _drawExplosionRange = false;

    private void Update()
    {
        // Check if in countdown
        if (_countdownStarted)
        {
            _timeRemaining -= Time.deltaTime;

            foreach (IProximityExplodableSubscriber subscriber in Subscribers)
            {
                subscriber.NotifyCountdownUpdated(gameObject, _timeRemaining);
            }

            // When countdown ends
            if (_timeRemaining <= .0f)
            {
                // Notify the subscriber before destroying the gameobject, because it would send a null gameonject if it was destroy first
                foreach (IProximityExplodableSubscriber subscriber in Subscribers)
                {
                    subscriber.NotifyCountdownFinished(gameObject);
                }
                
                DamageInRange();

                // Destroy the exploding GameObject
                Destroy(gameObject);

                // Show explosion effect
                Instantiate(_explosionEffect, _explosionPosition.position, Quaternion.identity);
            }
        }
        else
        {
            float distanceToTarget = ((_target.position - transform.position) + _distanceOffset).magnitude;

            // Check if close enough to trigger countdown
            if (distanceToTarget <= _distanceToCountdown)
            {
                _timeRemaining = _countdownTime;
                _countdownStarted = true;

                foreach (IProximityExplodableSubscriber subscriber in Subscribers)
                {
                    subscriber.NotifyCountdownStarted(gameObject, _timeRemaining);
                }
            }
        }
    }

    private void DamageInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + _distanceOffset, _explosionRange, _damagedLayers);

        foreach (Collider2D collider in colliders)
        {
            // Deal damage
            IDamageable health = collider.GetComponent<IDamageable>();
            health.Damage(_damage);

            switch (collider.tag)
            {
                case GameManager.PlayerTag:
                    // Knock back
                    float horizontalDirection = Mathf.Sign((collider.bounds.center - _explosionPosition.position).x);
                    Vector2 knockBackForce = new Vector2(horizontalDirection * Mathf.Abs(_knockBackDirection.x), Mathf.Abs(_knockBackDirection.y)).normalized * _knockBackStrength;
                    collider.GetComponent<PlatformerMovement>().KnockBack(knockBackForce, _knockBackDuration);
                    break;
            }
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void OnDrawGizmosSelected()
    {
        if (_drawDistanceToCountdown)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + _distanceOffset, _distanceToCountdown);
        }

        if (_drawExplosionRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position + _distanceOffset, _explosionRange);
        }
    }
}
