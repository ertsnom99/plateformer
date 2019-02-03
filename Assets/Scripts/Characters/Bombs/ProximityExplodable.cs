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
    private Transform m_target;
    [SerializeField]
    private float m_distanceToCountdown = 2.0f;
    [SerializeField]
    private Vector3 m_distanceOffset = Vector3.zero;
    [SerializeField]
    private float m_countdownTime = 5.0f;

    private bool m_countdownStarted = false;
    private float m_timeRemaining;

    [Header("Damage")]
    [SerializeField]
    private LayerMask m_damagedLayers;
    [SerializeField]
    private float m_explosionRange = 1.7f;
    [SerializeField]
    private int m_damage = 10;

    [Header("Explosion")]
    [SerializeField]
    private GameObject m_explosionEffect;
    [SerializeField]
    private Transform m_explosionPosition;

    [Header("Knock Back")]
    [SerializeField]
    private Vector3 m_knockBackDirection = new Vector3 (1.0f, 1.0f, .0f);
    [SerializeField]
    private float m_knockBackStrength = 12.0f;
    [SerializeField]
    private float m_knockBackDuration = .3f;

    [Header("Debug")]
    [SerializeField]
    private bool m_drawDistanceToCountdown = false;
    [SerializeField]
    private bool m_drawExplosionRange = false;

    private void Update()
    {
        // Check if in countdown
        if (m_countdownStarted)
        {
            m_timeRemaining -= Time.deltaTime;

            foreach (IProximityExplodableSubscriber subscriber in m_subscribers)
            {
                subscriber.NotifyCountdownUpdated(gameObject, m_timeRemaining);
            }

            // When countdown ends
            if (m_timeRemaining <= .0f)
            {
                // Notify the subscriber before destroying the gameobject, because it would send a null gameonject if it was destroy first
                foreach (IProximityExplodableSubscriber subscriber in m_subscribers)
                {
                    subscriber.NotifyCountdownFinished(gameObject);
                }
                
                DamageInRange();

                // Destroy the exploding GameObject
                Destroy(gameObject);

                // Show explosion effect
                Instantiate(m_explosionEffect, m_explosionPosition.position, Quaternion.identity);
            }
        }
        else
        {
            float distanceToTarget = ((m_target.position - transform.position) + m_distanceOffset).magnitude;

            // Check if close enough to trigger countdown
            if (distanceToTarget <= m_distanceToCountdown)
            {
                m_timeRemaining = m_countdownTime;
                m_countdownStarted = true;

                foreach (IProximityExplodableSubscriber subscriber in m_subscribers)
                {
                    subscriber.NotifyCountdownStarted(gameObject, m_timeRemaining);
                }
            }
        }
    }

    private void DamageInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position + m_distanceOffset, m_explosionRange, m_damagedLayers);

        foreach (Collider2D collider in colliders)
        {
            // Deal damage
            IDamageable health = collider.GetComponent<IDamageable>();
            health.Damage(m_damage);

            switch (collider.tag)
            {
                case GameManager.PlayerTag:
                    // Knock back
                    float horizontalDirection = Mathf.Sign((collider.bounds.center - m_explosionPosition.position).x);
                    Vector2 knockBackForce = new Vector2(horizontalDirection * Mathf.Abs(m_knockBackDirection.x), Mathf.Abs(m_knockBackDirection.y)).normalized * m_knockBackStrength;
                    collider.GetComponent<PlatformerMovement>().KnockBack(knockBackForce, m_knockBackDuration);
                    break;
            }
        }
    }

    public void SetTarget(Transform target)
    {
        m_target = target;
    }

    private void OnDrawGizmosSelected()
    {
        if (m_drawDistanceToCountdown)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + m_distanceOffset, m_distanceToCountdown);
        }

        if (m_drawExplosionRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position + m_distanceOffset, m_explosionRange);
        }
    }
}
