using UnityEngine;

public class SpikeRespawner : KnockBackRespawner, IPhysicsCollision2DListener
{
    [SerializeField]
    private Vector3 _kockBackDirection = new Vector3(.0f, 1.0f, .0f);

    protected override Vector3 CalculateKnockedBackDirection(Collider2D col)
    {
        return _kockBackDirection;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnTouchSpike(collision.collider);
    }

    // Methods of the IPhysicsCollision2DListener interface
    public void OnPhysicsCollision2DEnter(PhysicsCollision2D collision)
    {
        OnTouchSpike(collision.Collider);
    }

    public void OnPhysicsCollision2DExit(PhysicsCollision2D collision) { }

    public void OnPhysicsCollision2DStay(PhysicsCollision2D collision) { }
}
