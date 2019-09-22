using UnityEngine;

public interface ITriggerZoneSubscriber
{
    void NotifyTriggerEntered(TriggerZone triggerZone);
}

public class TriggerZone : MonoSubscribable<ITriggerZoneSubscriber>
{
    [Header("Activation")]
    [SerializeField]
    private bool m_enemyActivateTriggerZone = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag) || (m_enemyActivateTriggerZone && col.CompareTag(GameManager.EnemyTag) && col.GetComponent<PossessableCharacterController>().IsPossessed))
        {
            foreach(ITriggerZoneSubscriber subscriber in Subscribers)
            {
                subscriber.NotifyTriggerEntered(this);
            }
        }
    }
}
