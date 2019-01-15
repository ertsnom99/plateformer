using UnityEngine;

public interface IHealthSubscriber
{
    void NotifyJustSubscribed(Health healthScript);
    void NotifyDamageApplied(Health healthScript, int damage);
    void NotifyHealthChange(Health healthScript, int health);
    void NotifyHealthDepleted(Health healthScript);
}

public interface IDamageable
{
    void Damage(int damage);
}

public interface IHealable
{
    void Heal(int gain);
}

public class Health : MonoSubscribable<IHealthSubscriber>, IDamageable, IHealable
{
    [Header("Health")]
    [SerializeField]
    private int m_maxHealth = 100;
    public int MaxHealth
    {
        get { return m_maxHealth; }
        private set { m_maxHealth = value; }
    }

    public int HealthPoint { get; protected set; }

    private float m_invulnerabilityDuration;
    private float m_invulnerabilityStartTime;

    public bool IsInvulnerability { get; private set; }

    protected virtual void Awake()
    {
        HealthPoint = MaxHealth;
        IsInvulnerability = false;
    }

    protected virtual void Update()
    {
        // Update invulnerability
        if (IsInvulnerability && (Time.time - m_invulnerabilityStartTime) > m_invulnerabilityDuration)
        {
            IsInvulnerability = false;
        }
    }

    public void StartInvulnerability(float duration = Mathf.Infinity)
    {
        IsInvulnerability = true;

        m_invulnerabilityStartTime = Time.time;
        m_invulnerabilityDuration = duration;
    }

    public void StopInvulnerability()
    {
        IsInvulnerability = false;
    }

    protected virtual void OnDamageApplied() { }

    protected virtual void OnHealthDepleted() { }

    // Methods of the IDamageable interface
    public virtual void Damage(int damage)
    {
        foreach (IHealthSubscriber subscriber in m_subscribers)
        {
            subscriber.NotifyDamageApplied(this, damage);
        }

        if (!IsInvulnerability && damage > 0)
        {
            HealthPoint = HealthPoint - damage < 0 ? 0 : HealthPoint - damage;

            OnDamageApplied();

            if (HealthPoint == 0)
            {
                OnHealthDepleted();
            }

            foreach (IHealthSubscriber subscriber in m_subscribers)
            {
                subscriber.NotifyHealthChange(this, HealthPoint);

                if (HealthPoint == 0)
                {
                    subscriber.NotifyHealthDepleted(this);
                }
            }
        }
    }

    // Methods of the IHealable interface
    public virtual void Heal(int gain)
    {
        HealthPoint = HealthPoint + gain > MaxHealth ? MaxHealth : HealthPoint + gain;

        foreach (IHealthSubscriber subscriber in m_subscribers)
        {
            subscriber.NotifyHealthChange(this, HealthPoint);
        }
    }

    // Override the Subscribe method
    public override void Subscribe(IHealthSubscriber subscriber)
    {
        base.Subscribe(subscriber);
        subscriber.NotifyJustSubscribed(this);
    }
}
