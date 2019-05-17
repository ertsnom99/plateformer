using UnityEngine;

public interface IButtonSubscriber
{
    void NotifyButtonPressed(Button button);
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

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        _animator.SetBool(IsPressedParamHashId, _initiallyPressed);
        IsPressed = _initiallyPressed;
    }

    private void Press()
    {
        foreach (IButtonSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyButtonPressed(this);
        }

        _animator.SetBool(IsPressedParamHashId, true);
        IsPressed = true;
    }

    private void Unpress()
    {
        _animator.SetBool(IsPressedParamHashId, false);
        IsPressed = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsPressed && col.CompareTag(GameManager.PlayerTag))
        {
            Press();
        }
    }
}
