using UnityEngine;

public class PhysicsCollision2D
{
    public PhysicsCollision2D(Collider2D collider, Collider2D otherCollider, Rigidbody2D rigidbody, Rigidbody2D otherRigidbody, Transform transform, GameObject gameObject, Vector2 relativeVelocity, bool enabled, Vector2? contactPoint = null, Vector2? normal = null)
    {
        Collider = collider;
        OtherCollider = otherCollider;
        Rigidbody = rigidbody;
        OtherRigidbody = otherRigidbody;
        Transform = transform;
        GameObject = gameObject;
        RelativeVelocity = relativeVelocity;
        Enabled = enabled;

        if (contactPoint != null)
        {
            Contact = true;
            ContactPoint = (Vector2)contactPoint;
        }
        else
        {
            Contact = false;
        }

        if (normal != null)
        {
            Normal = (Vector2)normal;
        }
    }

    public Collider2D Collider { get; private set; }
    public Collider2D OtherCollider { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public Rigidbody2D OtherRigidbody { get; private set; }
    public Transform Transform { get; private set; }
    public GameObject GameObject { get; private set; }
    // Doesn't take in consideration the rotation
    public Vector2 RelativeVelocity { get; private set; }
    // See: https://docs.unity3d.com/ScriptReference/Collision2D-enabled.html
    public bool Enabled { get; private set; }
    public bool Contact { get; private set; }
    public Vector2 ContactPoint { get; private set; }
    public Vector2 Normal { get; private set; }
}
