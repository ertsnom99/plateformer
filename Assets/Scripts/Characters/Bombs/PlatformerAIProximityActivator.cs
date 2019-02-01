using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerAIControl))]
[RequireComponent(typeof(ProximityExplodable))]

public class PlatformerAIProximityActivator : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform m_target;
    [SerializeField]
    private float m_minDistance;

    private PlatformerAIControl m_controlScript;
    private ProximityExplodable m_explodableScript;

    private void Awake()
    {
        m_controlScript = GetComponent<PlatformerAIControl>();
        m_explodableScript = GetComponent<ProximityExplodable>();
    }

    private void Update()
    {
        if ((transform.position - m_target.position).magnitude <= m_minDistance)
        {
            m_controlScript.enabled = true;
            m_explodableScript.enabled = true;

            Destroy(this);
        }
    }

    public void SetTarget(Transform target)
    {
        m_target = target;
    }
}
