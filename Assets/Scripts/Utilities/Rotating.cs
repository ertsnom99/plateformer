using UnityEngine;

public class Rotating : MonoBehaviour
{
    [Header("Rotating")]
    [SerializeField]
    private float m_rotationSpeed = 200.0f;

    private void Update ()
    {
        transform.rotation *= Quaternion.Euler(0.0f, m_rotationSpeed * Time.deltaTime, 0.0f);
	}
}
