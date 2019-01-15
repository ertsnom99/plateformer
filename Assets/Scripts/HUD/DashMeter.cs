using UnityEngine;
using UnityEngine.UI;

public class DashMeter : MonoBehaviour, IPlatformerMovementSubscriber
{
    [Header("Platformer Movement")]
    [SerializeField]
    private PlatformerMovement m_platformerMovement;

    [Header("Ability Icone")]
    [SerializeField]
    private Image m_abilityIconeImage;
    [SerializeField]
    private Animator m_abilityIconeAnimator;

    [Header("Transition")]
    [SerializeField]
    private Color m_unavailableColor = Color.black;
    [SerializeField]
    private Color m_availableColor = Color.white;

    protected int m_cooldownFinishedParamHashId = Animator.StringToHash(CooldownFinishedParamNameString);

    public const string CooldownFinishedParamNameString = "CooldownFinished";

    private void Start()
    {
        if (!m_platformerMovement)
        {
            Debug.LogError("No PlatformerMovement script was set!");
        }
        else if(!m_abilityIconeImage)
        {
            Debug.LogError("No ability icone image was set!");
        }
        else if (!m_abilityIconeAnimator)
        {
            Debug.LogError("No ability icone animator was set!");
        }
        else
        {
            m_platformerMovement.Subscribe(this);
        }
    }

    // Methods of the IPlatformerMovementSubscriber interface
    public void NotifyDashUsed()
    {
        m_abilityIconeImage.color = m_unavailableColor;
    }

    public void NotifyDashCooldownUpdated(float cooldownProgress)
    {
        m_abilityIconeImage.color = Color.Lerp(m_unavailableColor, m_availableColor, cooldownProgress);
    }

    public void NotifyDashCooldownOver()
    {
        m_abilityIconeAnimator.SetTrigger(m_cooldownFinishedParamHashId);
    }
}
