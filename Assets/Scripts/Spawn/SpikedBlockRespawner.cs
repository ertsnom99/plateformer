using UnityEngine;

public class SpikedBlockRespawner : KnockedBackRespawner
{
    protected override Vector3 CalculateKnockedBackDirection(Collider2D col)
    {
        return col.bounds.center - transform.position;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        OnTouchSpike(col);
    }
}
