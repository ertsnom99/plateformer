using UnityEngine;

public interface ITriggerZoneSubscriber
{
    void NotifyTriggerEntered(TriggerZone triggerZone);
}

public class TriggerZone : MonoSubscribable<ITriggerZoneSubscriber>
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            foreach(ITriggerZoneSubscriber subscriber in m_subscribers)
            {
                subscriber.NotifyTriggerEntered(this);
            }
        }
    }
}
