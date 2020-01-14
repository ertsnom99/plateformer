using UnityEngine;

public class LoopingTranslate : MonoBehaviour
{
    [SerializeField]
    private Transform _start;
    [SerializeField]
    private Transform _end;
    [SerializeField]
    private float _speed = .01f;

    private void Awake()
    {
        if (!_start)
        {
            Debug.LogError("No start was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_end)
        {
            Debug.LogError("No end was set for " + GetType() + " script of " + gameObject.name + "!");
        }
    }

    private void Start()
    {
        transform.position = _start.position;
    }

    private void Update()
    {
        Vector3 movementDirection = (_end.position - transform.position).normalized;
        Vector3 remainingDistance = _end.position - transform.position;
        Vector3 movementDone = movementDirection * _speed * Time.deltaTime;
        Vector3 diff = remainingDistance - movementDone;

        // Loop if reached the end
        if (Vector3.Dot(diff, movementDirection) <= .0f)
        {
            transform.position = _start.position + movementDirection * diff.magnitude;
        }
        else
        {
            transform.position += movementDone;
        }
    }
}
