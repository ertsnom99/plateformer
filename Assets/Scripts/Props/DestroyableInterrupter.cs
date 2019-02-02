using UnityEngine;

public interface IDestroyableInterrupterSubscriber
{
    void NotifyInterrupterDestroyed(DestroyableInterrupter destroyableInterrupter);
}

public enum WayToBreak
{
    Jump = 0,
    Dash,
    Explosion
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Animator))]

public class DestroyableInterrupter : MonoSubscribable<IDestroyableInterrupterSubscriber>, IDamageable
{
    [Header("Break method")]
    [SerializeField]
    private WayToBreak m_wayToBreak;
    [SerializeField]
    private float m_velocityToBreak = 30.0f;

    public bool IsBreaked { get; private set; }

    private Animator m_animator;

    protected int m_isDestroyedParamHashId = Animator.StringToHash(IsDestroyedParamNameString);

    public const string IsDestroyedParamNameString = "IsDestroyed";

    private void Awake()
    {
        IsBreaked = false;

        m_animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsBreaked && col.CompareTag(GameManager.PlayerTag))
        {
            PlatformerMovement movementScript = col.GetComponent<PlatformerMovement>();

            switch (m_wayToBreak)
            {
                case WayToBreak.Jump:
                    float angle = Vector2.Dot(-transform.up, movementScript.Velocity);

                    if (angle < 90.0f && movementScript.Velocity.magnitude >= m_velocityToBreak)
                    {
                        Break();
                    }

                    break;
                case WayToBreak.Dash:
                    if (movementScript.IsDashing)
                    {
                        Break();
                    }

                    break;
            }
        }
    }

    private void Break()
    {
        foreach (IDestroyableInterrupterSubscriber subscriber in m_subscribers)
        {
            subscriber.NotifyInterrupterDestroyed(this);
        }

        IsBreaked = true;
        m_animator.SetBool(m_isDestroyedParamHashId, true);
    }

    // Methods of the IDamageable interface
    public void Damage(int damage)
    {
        if (!IsBreaked)
        {
            switch (m_wayToBreak)
            {
                case WayToBreak.Explosion:
                    Break();
                    break;
            }
        }
    }
}
