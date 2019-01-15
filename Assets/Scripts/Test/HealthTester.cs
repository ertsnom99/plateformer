using UnityEngine;

public class HealthTester : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private Health m_health;
    [SerializeField]
    private int m_healthChange;

    private void Update()
    {
		if (Input.anyKeyDown)
        {
            if (m_healthChange > 0)
            {
                m_health.Heal(m_healthChange);
            }
            else if (m_healthChange < 0)
            {
                m_health.Damage(Mathf.Abs(m_healthChange));
            }
        }
	}
}
