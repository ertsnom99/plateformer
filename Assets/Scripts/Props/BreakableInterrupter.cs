using UnityEngine;

public interface IBreakableInterrupterSubscriber
{
    void NotifyInterrupterBreaked(BreakableInterrupter breakableInterrupter);
}

public enum WayToBreak
{
    Jump = 0,
    Dash,
    Explosion
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Animator))]

public class BreakableInterrupter : MonoSubscribable<IBreakableInterrupterSubscriber>
{
    [Header("Break method")]
    [SerializeField]
    private WayToBreak m_wayToBreak;
    [SerializeField]
    private float m_velocityToBreak = 30.0f;

    public bool IsBreaked { get; private set; }

    private Animator m_animator;

    protected int m_isBreakedParamHashId = Animator.StringToHash(IsBreakedParamNameString);

    public const string IsBreakedParamNameString = "IsBreaked";

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
                        foreach (IBreakableInterrupterSubscriber subscriber in m_subscribers)
                        {
                            subscriber.NotifyInterrupterBreaked(this);
                        }

                        IsBreaked = true;
                        m_animator.SetBool(m_isBreakedParamHashId, true);
                    }

                    break;
                case WayToBreak.Dash:
                    if (movementScript.IsDashing)
                    {
                        foreach (IBreakableInterrupterSubscriber subscriber in m_subscribers)
                        {
                            subscriber.NotifyInterrupterBreaked(this);
                        }

                        IsBreaked = true;
                        m_animator.SetBool(m_isBreakedParamHashId, true);
                    }

                    break;
                case WayToBreak.Explosion:

                    //IsBreaked = true;
                    //m_animator.SetBool(m_isBreakedParamHashId, true);

                    break;
            }
        }
    }
}
