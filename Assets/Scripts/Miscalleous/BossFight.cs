using UnityEngine;

public class BossFight : MonoBehaviour, IHealthSubscriber
{
    [Header("Boss")]
    [SerializeField]
    private Health _bossHealth;

    private void Awake()
    {
        _bossHealth.Subscribe(this);
    }

    // Methods of the IHealable interface
    public void NotifyDamageCalled(Health healthScript, int damage) { }

    public void NotifyHealCalled(Health healthScript, int gain) { }

    public void NotifyHealthChange(Health healthScript, int health) { }

    public void NotifyHealthDepleted(Health healthScript)
    {
        ((DevGameManager)DevGameManager.Instance).EndGame(true);
    }

    public void NotifyJustSubscribed(Health healthScript) { }
}
