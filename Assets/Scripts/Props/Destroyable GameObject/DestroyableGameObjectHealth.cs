using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]

public class DestroyableGameObjectHealth : Health
{
    [SerializeField]
    private int _healthRegenRate = 10;

    [Header("Color")]
    [SerializeField]
    private Color _intactColor;
    [SerializeField]
    private Color _brokenColor;

    private SpriteRenderer _renderer;

    private bool _canBeDirectlyDamage = true;

    protected override void Awake()
    {
        base.Awake();

        _renderer = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    protected override void Update()
    {
        base.Update();

        if (HealthPoint > 0)
        {
            Heal((int)(Time.deltaTime * _healthRegenRate));
        }
    }

    public void SetCanBeDirectlyDamage(bool canBeDirectlyDamage)
    {
        _canBeDirectlyDamage = canBeDirectlyDamage;
    }

    public void ForceDamage(int damage)
    {
        base.Damage(damage);
    }

    public override void Damage(int damage)
    {
        if (_canBeDirectlyDamage)
        {
            base.Damage(damage);
        }
    }

    protected override void OnDamageDealt()
    {
        UpdateColor();
    }

    protected override void OnHealingDone()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        _renderer.color = Color.Lerp(_brokenColor, _intactColor, (float)HealthPoint / MaxHealth);
    }
}
