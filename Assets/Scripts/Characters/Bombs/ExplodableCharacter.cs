using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]

public class ExplodableCharacter : Explodable
{
    [Header("Knock Back Direction")]
    [SerializeField]
    private float _maxDifMagWhenClose = .1f;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override float CalculateHorizontalDirection(Collider2D damagedCollider)
    {
        if ((damagedCollider.bounds.center - ExplosionPosition.position).magnitude <= _maxDifMagWhenClose)
        {
            return _spriteRenderer.flipX ? -1.0f : 1.0f;
        }
        else
        {
            return base.CalculateHorizontalDirection(damagedCollider);
        }
    }
}
