using UnityEngine;

public class ExplodableBullet : Explodable
{
    [Header("Knock Back Direction")]
    [SerializeField]
    private float _maxDifMagWhenClose = .1f;

    protected override float CalculateHorizontalDirection(Collider2D damagedCollider)
    {
        if ((damagedCollider.bounds.center - ExplosionPosition.position).magnitude <= _maxDifMagWhenClose)
        {
            return Mathf.Sign(-transform.right.x);
        }
        else
        {
            return base.CalculateHorizontalDirection(damagedCollider);
        }
    }
}
