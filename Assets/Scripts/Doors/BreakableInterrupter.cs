using UnityEngine;

public interface IInterrupterBreakable
{
    void NotifyInterrupterBreaked();
}

public enum WayToBreak
{
    Jump = 0,
    Dash,
    Explosion
}

public class BreakableInterrupter : MonoSubscribable<IInterrupterBreakable>
{
    [Header("Break method")]
    [SerializeField]
    private WayToBreak m_wayToBreak;
    [SerializeField]
    private float m_velocityToBreak = 30.0f;
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            PlatformerMovement movementScript = col.GetComponent<PlatformerMovement>();

            switch (m_wayToBreak)
            {
                case WayToBreak.Jump:
                    float angle = Vector2.Dot(-transform.up, movementScript.Velocity);
                    
                    if (angle < 90.0f && movementScript.Velocity.magnitude >= m_velocityToBreak)
                    {
                        foreach (IInterrupterBreakable subscriber in m_subscribers)
                        {
                            subscriber.NotifyInterrupterBreaked();
                        }

                        Debug.Log("DESTROYED!");
                        //Destroy(this);
                    }

                    break;
                case WayToBreak.Dash:
                    if (movementScript.IsDashing)
                    {
                        foreach (IInterrupterBreakable subscriber in m_subscribers)
                        {
                            subscriber.NotifyInterrupterBreaked();
                        }

                        Debug.Log("DESTROYED!");
                        //Destroy(this);
                    }

                    break;
                case WayToBreak.Explosion:

                    break;
            }
        }
    }
}
