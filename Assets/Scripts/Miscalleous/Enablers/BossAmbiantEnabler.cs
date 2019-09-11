using UnityEngine;

public class BossAmbiantEnabler : MonoBehaviour
{
    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!_triggered && col.CompareTag(GameManager.PlayerTag))
        {
            _triggered = true;
            // TODO : Start transition to boss ambiant
            //AmbiantManager.Instance.StartBossAmbiant();
        }
    }
}
