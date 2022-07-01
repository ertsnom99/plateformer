using UnityEngine;

public class ExplodableBullet : Explodable
{
    protected override Vector2 CalculateKnockBackDirection(Collider2D damagedCollider)
    {
        return new Vector2(-transform.right.x, Mathf.Clamp01(-transform.right.y));
    }
}
