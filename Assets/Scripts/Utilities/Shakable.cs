using System.Collections;
using UnityEngine;

public class Shakable : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private float m_duration = 0.2f;
    [SerializeField]
    private float m_frequence = 0.01f;
    [SerializeField]
    private float m_strength = 0.8f;
    [SerializeField]
    private float m_angle = -4.0f;
    [SerializeField]
    private bool m_useRotation = true;
    [SerializeField]
    private Vector3 m_usedRotationAxis = new Vector3(1, 0, 1);

    private Vector3 m_initPos;
    private float m_remainingDuration = 0.0f;
    private Vector3 m_direction = new Vector3(0, 0, 0);
    private Vector3 m_axis = new Vector3(0, 0, 0);

    protected virtual void Awake()
    {
        m_initPos = transform.localPosition;
    }

    private void Update()
    {
        if (m_remainingDuration > 0)
        {
            float shakeProgress = m_remainingDuration / m_duration;

            transform.localPosition = m_initPos + new Vector3(m_direction.x, m_direction.y, 0);

            if (m_useRotation)
            {
                transform.localEulerAngles = m_axis * m_angle * shakeProgress;
            }

            m_remainingDuration -= Time.deltaTime;

            if (m_remainingDuration < 0.0f)
            {
                m_remainingDuration = 0.0f;

                transform.localPosition = m_initPos;

                if (m_useRotation)
                {
                    transform.localEulerAngles = Vector3.zero;
                }

                StopAllCoroutines();
            }
        }
    }

    public void Shake()
    {
        m_remainingDuration = m_duration;
        m_direction = GenerateShakeDirection();
        m_axis = GenerateShakeAxis();
        
        StartCoroutine(UpdateShakePos());
    }

    private IEnumerator UpdateShakePos()
    {
        while (true)
        {
            yield return new WaitForSeconds(m_frequence);

            m_direction = GenerateShakeDirection();
        }
    }

    private Vector3 GenerateShakeDirection()
    {
        float shakeProgress = m_remainingDuration / m_duration;
        return Random.insideUnitCircle * m_strength * shakeProgress;
    }

    private Vector3 GenerateShakeAxis()
    {
        return new Vector3(Mathf.Sign(Random.Range(0, 2) - 1) * m_usedRotationAxis.x, Mathf.Sign(Random.Range(0, 2) - 1) * m_usedRotationAxis.y, Mathf.Sign(Random.Range(0, 2) - 1) * m_usedRotationAxis.z);
    }
}
