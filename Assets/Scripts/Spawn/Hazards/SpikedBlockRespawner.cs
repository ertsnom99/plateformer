using UnityEngine;

public class SpikedBlockRespawner : KnockBackRespawner, IPhysicsCollision2DListener
{
    protected override Vector3 CalculateKnockedBackDirection(Collider2D col)
    {
        return col.bounds.center - transform.position;
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
