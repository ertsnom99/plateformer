using UnityEngine;

public class RetractableSpikeRespawner : KnockBackRespawner
{
    protected override Vector3 CalculateKnockedBackDirection(Collider2D col)
    {
        Vector3 knockedBackDirection = transform.up + (col.transform.position - transform.position);
        knockedBackDirection = new Vector3(Mathf.Sign(knockedBackDirection.x), Mathf.Sign(knockedBackDirection.y), .0f).normalized;

        return knockedBackDirection;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        OnTouchSpike(col);
    }
}
