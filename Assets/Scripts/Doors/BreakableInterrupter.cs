using UnityEngine;

public interface IInterrupterBreakable
{
    void NotifyInterrupterBreaked();
}

public class BreakableInterrupter : MonoSubscribable<IInterrupterBreakable>
{
    private void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log(gameObject.name + " Enter " + col.contacts.Length);
        /*foreach (ContactPoint2D contact in col.contacts)
        {
            Debug.Log(gameObject.name + " OnCollisionEnter2D " + contact.normal + " " + contact.otherCollider.gameObject);
        }*/
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        Debug.Log(gameObject.name + " Exit " + col.contacts.Length);
        /*foreach (ContactPoint2D contact in col.contacts)
        {
            Debug.Log(gameObject.name + " OnCollisionExit2D " + contact.normal);
        }*/
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("OnTriggerEnter2D");
    }
}
