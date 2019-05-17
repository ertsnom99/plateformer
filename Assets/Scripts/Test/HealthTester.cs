using UnityEngine;

public class HealthTester : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private Health _health;
    [SerializeField]
    private int _healthChange;

    private void Update()
    {
		if (Input.anyKeyDown)
        {
            if (_healthChange > 0)
            {
                _health.Heal(_healthChange);
            }
            else if (_healthChange < 0)
            {
                _health.Damage(Mathf.Abs(_healthChange));
            }
        }
	}
}
