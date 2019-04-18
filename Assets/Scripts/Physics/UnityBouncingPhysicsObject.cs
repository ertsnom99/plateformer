using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class UnityBouncingPhysicsObject : MonoBehaviour
{
    enum BounceStopCondition{ BounceDurationElapsed, MaxBounceCountReached, UnderMinVelocity };

    [Header("Bounce")]
    [SerializeField]
    private float m_bounceFreezeDuration = .1f;
    [SerializeField]
    private BounceStopCondition m_bounceStopCondition;
    [SerializeField]
    private float m_bounceDuration = 3.0f;
    private float m_bounceElapsedTime = .0f;
    [SerializeField]
    private int m_maxBounceCount = 10;
    [SerializeField]
    private float m_minVelocity = 5.0f;

    [SerializeField]
    private Vector2 m_force = new Vector2(-10.0f, -2.0f);

    private bool m_collisionHappened = false;
    private bool m_movementFrozen = false;
    private int m_bounceCount = 0;
    private Vector2 m_test;
    private Rigidbody2D m_rigidbody2D;

    private void Awake()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();

        InitialiseRigidbody2D();
    }

    private void InitialiseRigidbody2D()
    {
        m_rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        m_rigidbody2D.simulated = true;
        m_rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        m_rigidbody2D.sleepMode = RigidbodySleepMode2D.NeverSleep;
        m_rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;
        m_rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        Freeze();
    }

    public void Throw(Vector2 force)
    {
        m_bounceElapsedTime = .0f;
        m_bounceCount = 0;

        UnFreeze();
        m_rigidbody2D.AddForce(force, ForceMode2D.Impulse);
    }

    private void Update()
    {
        if (!m_movementFrozen)
        {
            m_bounceElapsedTime += Time.deltaTime;

            if (m_bounceStopCondition == BounceStopCondition.BounceDurationElapsed && m_bounceElapsedTime >= m_bounceDuration)
            {
                Freeze();
            }
            else if (m_bounceStopCondition == BounceStopCondition.UnderMinVelocity && m_rigidbody2D.velocity.magnitude < m_minVelocity)
            {
                Freeze();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!m_movementFrozen)
        {
            m_collisionHappened = false;

            // Rotate to be oriented toward Velocity direction
            float angle = Mathf.Atan2(m_rigidbody2D.velocity.y, m_rigidbody2D.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!m_movementFrozen && !m_collisionHappened)
        {
            m_collisionHappened = true;
            m_bounceCount++;

            if (m_bounceStopCondition == BounceStopCondition.MaxBounceCountReached && m_bounceCount >= m_maxBounceCount)
            {
                Freeze();
            }
            else if (!m_movementFrozen)
            {
                StartCoroutine(FreezeForDuration());
            }
        }
    }

    private void Freeze()
    {
        m_movementFrozen = true;
        m_test = m_rigidbody2D.velocity;
        m_rigidbody2D.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
    }

    private void UnFreeze()
    {
        m_rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        m_rigidbody2D.velocity = m_test;
        m_movementFrozen = false;
    }

    private IEnumerator FreezeForDuration()
    {
        Freeze();
        OnFreezeStart();

        yield return new WaitForSeconds(m_bounceFreezeDuration);

        UnFreeze();
        OnFreezeEnd();
    }

    protected virtual void OnFreezeStart() { }

    protected virtual void OnFreezeEnd() { }
}
