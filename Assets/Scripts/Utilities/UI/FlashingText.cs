using UnityEngine;
using UnityEngine.UI;

public class FlashingText : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField]
    private float m_flashDuration = 1.0f;
    [SerializeField]
    private bool m_useStartTimeHasReference;
    private float m_startTime;

    private Text m_text;

    private void Awake()
    {
        m_text = GetComponent<Text>();
    }

    private void Start()
    {
        if (m_useStartTimeHasReference)
        {
            m_startTime = Time.time;
        }
    }

    private void Update()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        float alpha = Mathf.Lerp(0.0f, 1.0f, Mathf.PingPong((Time.time - m_startTime) / m_flashDuration, 1));
        m_text.color = new Color(m_text.color.r, m_text.color.g, m_text.color.b, alpha);
    }

    private void OnEnable()
    {
        if (!m_useStartTimeHasReference)
        {
            m_startTime = Time.time;
        }
        else
        {
            UpdateColor();
        }
    }

    private void OnDisable()
    {
        m_text.color = new Color(m_text.color.r, m_text.color.g, m_text.color.b, 0.0f);
    }
}
