using UnityEngine;

public interface IButtonSubscriber
{
    void NotifyButtonPressed(Button button);
    void NotifyButtonUnpressed(Button button);
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Animator))]

public class Button : MonoSubscribable<IButtonSubscriber>
{
    [Header("Initialization")]
    [SerializeField]
    private bool _initiallyPressed = false;

    public bool IsPressed { get; private set; }

    private Animator _animator;

    protected int IsPressedParamHashId = Animator.StringToHash(IsPressedParamNameString);

    public const string IsPressedParamNameString = "IsPressed";

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();

        _animator.SetBool(IsPressedParamHashId, _initiallyPressed);
        IsPressed = _initiallyPressed;
    }

    protected virtual void Press()
    {
        _animator.SetBool(IsPressedParamHashId, true);
        IsPressed = true;

        foreach (IButtonSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyButtonPressed(this);
        }
    }

    protected virtual void Unpress()
    {
        _animator.SetBool(IsPressedParamHashId, false);
        IsPressed = false;

        foreach (IButtonSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyButtonUnpressed(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsPressed && CanPressButton(col))
        {
            Press();
        }
    }

    protected bool CanPressButton(Collider2D collider)
    {
        return collider.CompareTag(GameManager.PlayerTag) || collider.CompareTag(GameManager.EnemyTag);
    }
}
