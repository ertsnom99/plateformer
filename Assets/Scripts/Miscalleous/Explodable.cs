using UnityEngine;

public interface IProximityExplodableSubscriber
{
    void NotifyCountdownStarted(GameObject explodableGameObject, float timeRemaining);
    void NotifyCountdownUpdated(GameObject explodableGameObject, float timeRemaining);
    void NotifyCountdownFinished(GameObject explodableGameObject);
    void NotifyExploded(GameObject explodableGameObject);
}

public class Explodable : MonoSubscribable<IProximityExplodableSubscriber>
{
    [Header("Countdown")]
    [SerializeField]
    protected bool UseCountdown = true;
    [SerializeField]
    protected float CountdownTime = 3.0f;

    public bool CountdownStarted { protected set; get; }
    protected float TimeRemaining;

    [Header("Damage")]
    [SerializeField]
    protected Vector3 DistanceOffset = Vector3.zero;
    [SerializeField]
    private float _explosionRange = 1.7f;
    [SerializeField]
    private LayerMask _damagedLayers;
    [SerializeField]
    private int _damage = 10;

    [Header("Explosion")]
    [SerializeField]
    private GameObject _explosionEffect;
    [SerializeField]
    protected Transform ExplosionPosition;

    [Header("Knock Back")]
    [SerializeField]
    private Vector3 _knockBackDirection = new Vector3 (1.0f, 1.0f, .0f);
    [SerializeField]
    private float _knockBackStrength = 12.0f;
    [SerializeField]
    private float _knockBackDuration = .3f;

    [Header("Debug")]
    [SerializeField]
    private bool _drawExplosionRange = false;

    private void Awake()
    {
        CountdownStarted = false;
    }

    protected virtual void Update()
    {
        // Check if in countdown
        if (UseCountdown && CountdownStarted)
        {
            TimeRemaining -= Time.deltaTime;

            foreach (IProximityExplodableSubscriber subscriber in Subscribers)
            {
                subscriber.NotifyCountdownUpdated(gameObject, TimeRemaining);
            }

            // When countdown ends
            if (TimeRemaining <= .0f)
            {
                // Notify the subscriber before destroying the gameobject, because it would send a null gameonject if it was destroy first
                foreach (IProximityExplodableSubscriber subscriber in Subscribers)
                {
                    subscriber.NotifyCountdownFinished(gameObject);
                }

                Explode();
            }
        }
    }

    public void StartCountdown()
    {
        TimeRemaining = CountdownTime;
        CountdownStarted = true;

        foreach (IProximityExplodableSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyCountdownStarted(gameObject, TimeRemaining);
        }
    }

    public void Explode()
    {
        // Notify the subscriber before destroying the gameobject, because it would send a null gameonject if it was destroy first
        foreach (IProximityExplodableSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyExploded(gameObject);
        }

        DamageInRange();

        // Destroy the exploding GameObject
        Destroy(gameObject);

        // Show explosion effect
        Instantiate(_explosionEffect, ExplosionPosition.position, Quaternion.identity);
    }

    private void DamageInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + DistanceOffset, _explosionRange, _damagedLayers);

        foreach (Collider2D collider in colliders)
        {
            // Deal damage
            IDamageable health = collider.GetComponent<IDamageable>();
            health.Damage(_damage);

            switch (collider.tag)
            {
                case GameManager.PlayerTag:
                    // Knock back
                    float horizontalDirection = CalculateHorizontalDirection(collider);
                    Vector2 knockBackForce = new Vector2(horizontalDirection * Mathf.Abs(_knockBackDirection.x), Mathf.Abs(_knockBackDirection.y)).normalized * _knockBackStrength;
                    collider.GetComponent<PlatformerMovement>().KnockBack(knockBackForce, _knockBackDuration);
                    break;
            }
        }
    }

    protected virtual float CalculateHorizontalDirection(Collider2D damagedCollider)
    {
        return Mathf.Sign((damagedCollider.bounds.center - ExplosionPosition.position).x);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (_drawExplosionRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position + DistanceOffset, _explosionRange);
        }
    }
}
