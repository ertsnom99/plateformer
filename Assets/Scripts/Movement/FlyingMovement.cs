using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]

public class FlyingMovement : MonoBehaviour
{
    protected enum FlipType { Sprite, Scale };

    [Header("Rigidbody Settings")]
    [SerializeField]
    private float _drag = 7.5f;
    [SerializeField]
    private float _gravityScale = .0f;

    [Header("Movement")]
    [SerializeField]
    protected float Speed = 45.0f;

    protected Inputs CurrentInputs;

    [Header("Visual")]
    [SerializeField]
    protected FlipType FlipMethod = FlipType.Sprite;

    protected SpriteRenderer SpriteRenderer;
    protected Rigidbody2D Rigidbody;

    protected virtual void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void InitialiseRigidbody()
    {
        Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        Rigidbody.simulated = true;
        Rigidbody.useAutoMass = false;
        Rigidbody.drag = _drag;
        Rigidbody.gravityScale = _gravityScale;
        Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        Rigidbody.sleepMode = RigidbodySleepMode2D.StartAwake;
        Rigidbody.interpolation = RigidbodyInterpolation2D.None;
        Rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    protected virtual void FixedUpdate()
    {
        Rigidbody.AddForce(new Vector2(CurrentInputs.Horizontal, CurrentInputs.Vertical) * Speed - Rigidbody.velocity);

        Animate();
    }

    protected virtual void Animate()
    {
        if (ShouldFlip())
        {
            Flip();
        }
    }

    protected bool ShouldFlip()
    {
        if (CurrentInputs.Horizontal > -.01f && CurrentInputs.Horizontal < .01f)
        {
            return false;
        }

        return IsLookingForward() == CurrentInputs.Horizontal < .0f;
    }

    public bool IsLookingForward()
    {
        bool lookingForward = true;

        switch (FlipMethod)
        {
            case FlipType.Sprite:
                lookingForward = !SpriteRenderer.flipX;
                break;
            case FlipType.Scale:
                lookingForward = transform.localScale.x > 0;
                break;
        }

        return lookingForward;
    }

    protected void Flip()
    {
        switch (FlipMethod)
        {
            case FlipType.Sprite:
                SpriteRenderer.flipX = !SpriteRenderer.flipX;
                break;
            case FlipType.Scale:
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                break;
        }
    }

    public virtual void SetInputs(Inputs inputs)
    {
        CurrentInputs = inputs;
    }

    protected virtual void OnEnable()
    {
        InitialiseRigidbody();
    }

    protected virtual void OnDisable()
    {
        Rigidbody.velocity = Vector2.zero;
    }
}
