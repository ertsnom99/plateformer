using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]

public class FlyingMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float _speed = 45.0f;

    private Inputs _currentInputs;

    protected SpriteRenderer SpriteRenderer;
    protected Rigidbody2D Rigidbody;

    protected virtual void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Rigidbody.AddForce(new Vector2(_currentInputs.Horizontal, _currentInputs.Vertical) * _speed - Rigidbody.velocity);

        Animate();
    }

    protected virtual void Animate()
    {
        if (ShouldFlipSprite())
        {
            SpriteRenderer.flipX = !SpriteRenderer.flipX;
        }
    }

    protected bool ShouldFlipSprite()
    {
        return !SpriteRenderer.flipX ? (Rigidbody.velocity.x < -.01f) : (Rigidbody.velocity.x > .01f);
    }

    public void SetInputs(Inputs inputs)
    {
        _currentInputs = inputs;
    }
}
