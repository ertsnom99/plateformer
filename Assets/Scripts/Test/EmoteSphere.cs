using UnityEngine;

public class EmoteSphere : MonoBehaviour, IPhysicsCollision2DListener
{
    [Header("Emote")]
    [SerializeField]
    private SpriteRenderer _emote;
    [SerializeField]
    private Sprite[] _emotes;
    private int _emoteIndex = 0;

    [Header("Color")]
    [SerializeField]
    private SpriteRenderer _colorBackground;
    [SerializeField]
    private Color _intactColor;
    [SerializeField]
    private Color _brokenColor;

    private void Awake()
    {
        if (!_emote)
        {
            Debug.LogError("m_emoteSpriteRenderer isn't set for " + GetType() + " script of " + gameObject.name + "!!!");
        }

        if (!_colorBackground)
        {
            Debug.LogError("m_colorBackground isn't set for " + GetType() + " script of " + gameObject.name + "!!!");
        }

        _emote.sprite = _emotes[_emoteIndex];
        UpdateEmote();
    }

    private void UpdateEmote()
    {
        _emote.sprite = _emotes[_emoteIndex];
        _colorBackground.color = Color.Lerp(_intactColor, _brokenColor, (float)_emoteIndex / (_emotes.Length - 1));
    }

    // Methods of the IPhysicsObjectCollisionListener interface
    public void OnPhysicsCollision2DEnter(PhysicsCollision2D collision)
    {
        _emoteIndex = (_emoteIndex + 1) % _emotes.Length;
        UpdateEmote();
    }

    public void OnPhysicsCollision2DExit(PhysicsCollision2D collision) { }
}
