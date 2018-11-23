using UnityEngine;
using UnityEngine.UI;

public class FlashingImage : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField]
    private float m_flashDuration = 1.0f;
    [SerializeField]
    private bool m_useStartTimeHasReference;
    private float m_startTime;

    private Image m_image;

    private void Awake()
    {
        m_image = GetComponent<Image>();
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
        m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, alpha);
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
        m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, 0.0f);
    }
}
