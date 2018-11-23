using UnityEngine;

public class GrowOut : MonoBehaviour
{
    [Header("Pulsation")]
    [SerializeField]
    private float m_minScale = 0.1f;
    [SerializeField]
    private float m_maxScale = 1.0f;
    private float m_scaleDiff;
    [SerializeField]
    private float m_growDuration = 0.2f;
    [SerializeField]
    private AnimationCurve m_pgrowOutMovement;

    private bool m_growing = false;
    private float m_startTime;

    private void Awake()
    {
        m_scaleDiff = m_maxScale - m_minScale;
    }

    private void Start()
    {
        ResetScale();
    }

    private void Update()
    {
        if (m_growing)
        {
            float progress = (Time.time - m_startTime) / m_growDuration;

            if (progress > 1.0f)
            {
                progress = 1.0f;
                m_growing = false;
            }

            float scale = m_minScale + m_pgrowOutMovement.Evaluate(progress) * m_scaleDiff;
            transform.localScale = new Vector3(scale, scale, 1.0f);
        }
    }

    public void StartGrow()
    {
        ResetScale();

        m_startTime = Time.time;
        m_growing = true;
    }

    private void ResetScale()
    {
        transform.localScale = new Vector3(m_minScale, m_minScale, 1.0f);
    }
}
