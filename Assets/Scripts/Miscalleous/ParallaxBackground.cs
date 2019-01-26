using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField]
    private Transform m_followTarget;

    [SerializeField]
    private float m_XParallaxFactor;
    [SerializeField]
    private float m_YParallaxFactor;

    private Vector3 m_targetPreviousPos;

    private void Awake()
    {
        if (m_followTarget)
        {
            m_targetPreviousPos = m_followTarget.position;
        }
    }

    private void Update()
    {
        if (m_followTarget)
        {
            Vector3 movement = m_followTarget.position - m_targetPreviousPos;
            Vector3 parallaxMovement = new Vector3(movement.x * m_XParallaxFactor, movement.y * m_YParallaxFactor, .0f);
            transform.position += parallaxMovement;
            m_targetPreviousPos = m_followTarget.position;
        }
    }
}
