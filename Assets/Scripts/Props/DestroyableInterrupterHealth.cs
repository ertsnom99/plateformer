using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]

public class DestroyableInterrupterHealth : Health
{
    [SerializeField]
    private int m_healthRegenRate = 10;

    [Header("Color")]
    [SerializeField]
    private Color m_intactColor;
    [SerializeField]
    private Color m_brokenColor;

    private SpriteRenderer m_renderer;

    private bool m_canBeDirectlyDamage = true;

    protected override void Awake()
    {
        base.Awake();

        m_renderer = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    protected override void Update()
    {
        base.Update();

        if (HealthPoint > 0)
        {
            Heal((int)(Time.deltaTime * m_healthRegenRate));
        }
    }

    public void SetCanBeDirectlyDamage(bool canBeDirectlyDamage)
    {
        m_canBeDirectlyDamage = canBeDirectlyDamage;
    }

    public void ForceDamage(int damage)
    {
        base.Damage(damage);
    }

    public override void Damage(int damage)
    {
        if (m_canBeDirectlyDamage)
        {
            base.Damage(damage);
        }
    }

    protected override void OnDamageApplied()
    {
        UpdateColor();
    }

    protected override void OnHealed()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        m_renderer.color = Color.Lerp(m_brokenColor, m_intactColor, (float)HealthPoint / MaxHealth);
    }
}
