using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AnimatorEventsManager))]
[RequireComponent(typeof(BoxCollider2D))]

public class Door : MonoBehaviour, IAnimatorEventSubscriber
{
    [Header("Initialization")]
    [SerializeField]
    private bool _initiallyOpen = false;

    private Animator _animator;
    private AnimatorEventsManager _animatorEventsManager;
    private BoxCollider2D _collider;

    protected int IsOpenParamHashId = Animator.StringToHash(IsOpenParamNameString);

    public const string IsOpenParamNameString = "IsOpen";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animatorEventsManager = GetComponent<AnimatorEventsManager>();
        _collider = GetComponent<BoxCollider2D>();
        
        _animatorEventsManager.Subscribe(AnimatorEvents.AnimationBegin, this);
        _animatorEventsManager.Subscribe(AnimatorEvents.AnimationFinish, this);
    }

    private void Start()
    {
        _animator.SetBool(IsOpenParamHashId, _initiallyOpen);
    }

    public void Open()
    {
        _animator.SetBool(IsOpenParamHashId, true);
    }

    public void Close()
    {
        _animator.SetBool(IsOpenParamHashId, false);
    }

    // Methods of the IAnimatorEventSubscriber interface
    public void NotifyEvent(string eventName)
    {
        switch (eventName)
        {
            // When the door start to close
            case AnimatorEvents.AnimationBegin:
                _collider.enabled = true;
                break;
            // When the door finish to open
            case AnimatorEvents.AnimationFinish:
                _collider.enabled = false;
                break;
        }
    }
}
