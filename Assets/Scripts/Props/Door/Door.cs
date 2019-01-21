using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AnimatorEventsManager))]
[RequireComponent(typeof(BoxCollider2D))]

public class Door : MonoBehaviour, IAnimatorEventSubscriber
{
    [Header("Initialization")]
    [SerializeField]
    private bool m_initiallyOpen = false;

    private Animator m_animator;
    private AnimatorEventsManager m_animatorEventsManager;
    private BoxCollider2D m_collider;

    protected int m_isOpenParamHashId = Animator.StringToHash(IsOpenParamNameString);

    public const string IsOpenParamNameString = "IsOpen";

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_animatorEventsManager = GetComponent<AnimatorEventsManager>();
        m_collider = GetComponent<BoxCollider2D>();
        
        m_animatorEventsManager.Subscribe(AnimatorEvents.AnimationBegin, this);
        m_animatorEventsManager.Subscribe(AnimatorEvents.AnimationFinish, this);
    }

    private void Start()
    {
        m_animator.SetBool(m_isOpenParamHashId, m_initiallyOpen);
    }

    public void Open()
    {
        m_animator.SetBool(m_isOpenParamHashId, true);
    }

    public void Close()
    {
        m_animator.SetBool(m_isOpenParamHashId, false);
    }

    // Methods of the IAnimatorEventSubscriber interface
    public void NotifyEvent(string eventName)
    {
        switch (eventName)
        {
            // When the door start to close
            case AnimatorEvents.AnimationBegin:
                m_collider.enabled = true;
                break;
            // When the door finish to open
            case AnimatorEvents.AnimationFinish:
                m_collider.enabled = false;
                break;
        }
    }
}
