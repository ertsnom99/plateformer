using UnityEngine;

public class EmoteSphere : MonoBehaviour
{
    [Header("Emote")]
    [SerializeField]
    private SpriteRenderer m_emote;
    [SerializeField]
    private Sprite[] m_emotes;
    private int m_emoteIndex = 0;

    [Header("Color")]
    [SerializeField]
    private SpriteRenderer m_colorBackground;
    [SerializeField]
    private Color m_intactColor;
    [SerializeField]
    private Color m_brokenColor;
    
    private void Awake()
    {
        if (!m_emote)
        {
            Debug.LogError("m_emoteSpriteRenderer isn't set!!!");
        }

        if (!m_colorBackground)
        {
            Debug.LogError("m_colorBackground isn't set!!!");
        }

        m_emote.sprite = m_emotes[m_emoteIndex];
        UpdateEmote();
    }

    private void OnPhysicsObjectCollisionEnter(PhysicsCollision2D physicsObjectCollision2D)
    {
        m_emoteIndex = (m_emoteIndex + 1) % m_emotes.Length;
        UpdateEmote();
    }

    private void UpdateEmote()
    {
        m_emote.sprite = m_emotes[m_emoteIndex];
        m_colorBackground.color = Color.Lerp(m_intactColor, m_brokenColor, (float)m_emoteIndex / (m_emotes.Length - 1));
    }
}
