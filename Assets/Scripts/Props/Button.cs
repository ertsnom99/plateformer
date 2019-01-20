using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Animator))]

public abstract class Button : MonoBehaviour
{
    [Header("Initialization")]
    [SerializeField]
    private bool m_initiallyPressed = false;

    public bool IsPressed { get; private set; }

    private Animator m_animator;

    protected int m_isPressedParamHashId = Animator.StringToHash(IsPressedParamNameString);

    public const string IsPressedParamNameString = "IsPressed";

    private void Awake()
    {
        m_animator = GetComponent<Animator>();

        m_animator.SetBool(m_isPressedParamHashId, m_initiallyPressed);
        IsPressed = m_initiallyPressed;
    }

    private void Press()
    {
        m_animator.SetBool(m_isPressedParamHashId, true);
        IsPressed = true;
    }

    private void Unpress()
    {
        m_animator.SetBool(m_isPressedParamHashId, false);
        IsPressed = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsPressed && col.CompareTag(GameManager.PlayerTag))
        {
            Press();
            OnUse();
        }
    }

    protected abstract void OnUse();
}
