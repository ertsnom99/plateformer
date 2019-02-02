using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(ProximityExplodable))]

public class AIProximityActivator : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Transform m_target;
    [SerializeField]
    private float m_maxDistance;

    [Header("Debug")]
    [SerializeField]
    private bool m_drawDistance = false;

    private AIControl m_controlScript;
    private ProximityExplodable m_explodableScript;

    private void Awake()
    {
        m_controlScript = GetComponent<AIControl>();
        m_explodableScript = GetComponent<ProximityExplodable>();

        if (!m_controlScript)
        {
            Debug.LogError("No AIControl script was found!");
        }
    }

    private void Update()
    {
        if ((transform.position - m_target.position).magnitude <= m_maxDistance)
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

    private void OnDrawGizmosSelected()
    {
        if (m_drawDistance)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, m_maxDistance);
        }
    }
}
