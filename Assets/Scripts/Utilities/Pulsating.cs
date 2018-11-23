using UnityEngine;

public class Pulsating : MonoBehaviour
{
    [Header("Pulsation")]
    [SerializeField]
    private float m_pulsationScale = 1.1f;
    private Vector3 m_initScale;
    [SerializeField]
    private float m_pulseDuration = 1.0f;
    [SerializeField]
    private AnimationCurve m_pulsationMovement;
    [SerializeField]
    private bool m_useStartTimeHasReference;
    private float m_startTime;

    private void Awake()
    {
        m_initScale = transform.localScale;
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
        UpdateLocalScale();
    }

    private void UpdateLocalScale()
    {
        float scale = m_pulsationMovement.Evaluate(Mathf.PingPong((Time.time - m_startTime) / (m_pulseDuration / 2), 1)) * m_pulsationScale;
        transform.localScale = new Vector3(m_initScale.x + scale, m_initScale.y + scale, m_initScale.z + scale);
    }

    private void OnEnable()
    {
        if (!m_useStartTimeHasReference)
        {
            m_startTime = Time.time;
        }
        else
        {
            UpdateLocalScale();
        }
    }

    private void OnDisable()
    {
        transform.localScale = m_initScale;
    }
}
