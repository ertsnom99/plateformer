using UnityEngine;

public class CollisionRespawn : MonoBehaviour, IPhysicsCollision2DListener
{
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag(GameManager.PlayerTag))
        {
            col.transform.position = SpawnManager.Instance.SpawnPosition;
        }
    }

    // Methods of the IPhysicsCollision2DListener interface
    public void OnPhysicsCollision2DEnter(PhysicsCollision2D collision)
    {
        if (collision.Collider.CompareTag(GameManager.PlayerTag))
        {
            collision.Transform.position = SpawnManager.Instance.SpawnPosition;
        }
    }

    public void OnPhysicsCollision2DStay(PhysicsCollision2D collision) { }

    public void OnPhysicsCollision2DExit(PhysicsCollision2D collision) { }
}
