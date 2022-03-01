using UnityEngine;

public class ProximityExplodable : Explodable
{
    [SerializeField]
    private bool _drawDistanceToCountdown = false;

    [Header("Proximity")]
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private float _distanceToCountdown = 6.0f;

    protected override void Update()
    {
        base.Update();

        if (!CountdownStarted && _target)
        {
            float distanceToTarget = ((_target.position - transform.position) + DistanceOffset).magnitude;

            // Check if close enough to trigger countdown
            if (distanceToTarget <= _distanceToCountdown)
            {
                StartCountdown();
            }
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (_drawDistanceToCountdown)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + DistanceOffset, _distanceToCountdown);
        }
    }
}
