using UnityEngine;

public class ChangingPlatformOnTouch : ChangingPlatform, IPhysicsCollision2DListener
{
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag(GameManager.PlayerTag))
        {
            StartChanging();
        }
    }

    // Methods of the IPhysicsCollision2DListener interface
    public void OnPhysicsCollision2DEnter(PhysicsCollision2D collision)
    {
        if (collision.GameObject.CompareTag(GameManager.PlayerTag))
        {
            StartChanging();
        }
    }

    public void OnPhysicsCollision2DExit(PhysicsCollision2D collision) { }

    public void OnPhysicsCollision2DStay(PhysicsCollision2D collision) { }
}
