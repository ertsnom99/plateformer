using UnityEngine;

public class SpikeRespawner : KnockedBackRespawner
{
    [SerializeField]
    private Vector3 _kockBackDirection = new Vector3(.0f, 1.0f, .0f);

    protected override Vector3 CalculateKnockedBackDirection(Collider2D col)
    {
        return _kockBackDirection;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        OnTouchSpike(col);
    }
}
