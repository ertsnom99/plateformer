using UnityEngine;
using UnityEngine.UI;

public class DashMeter : MonoBehaviour, IPlatformerMovementSubscriber
{
    [Header("Platformer Movement")]
    [SerializeField]
    private PlatformerMovement _platformerMovement;

    [Header("Ability Icone")]
    [SerializeField]
    private Image _abilityIconeImage;
    [SerializeField]
    private Animator _abilityIconeAnimator;

    [Header("Transition")]
    [SerializeField]
    private Color _unavailableColor = Color.black;
    [SerializeField]
    private Color _availableColor = Color.white;

    protected int CooldownFinishedParamHashId = Animator.StringToHash(CooldownFinishedParamNameString);

    public const string CooldownFinishedParamNameString = "CooldownFinished";

    private void Start()
    {
        if (!_platformerMovement)
        {
            Debug.LogError("No PlatformerMovement script was set!");
        }
        else if(!_abilityIconeImage)
        {
            Debug.LogError("No ability icone image was set!");
        }
        else if (!_abilityIconeAnimator)
        {
            Debug.LogError("No ability icone animator was set!");
        }
        else
        {
            _platformerMovement.Subscribe(this);
        }
    }

    // Methods of the IPlatformerMovementSubscriber interface
    public void NotifyDashUsed()
    {
        _abilityIconeImage.color = _unavailableColor;
    }

    public void NotifyDashCooldownUpdated(float cooldownProgress)
    {
        _abilityIconeImage.color = Color.Lerp(_unavailableColor, _availableColor, cooldownProgress);
    }

    public void NotifyDashCooldownOver()
    {
        _abilityIconeAnimator.SetTrigger(CooldownFinishedParamHashId);
    }
}
