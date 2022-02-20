using UnityEngine;

public class PropelledFlyingCharacterController : FlyingCharacterController
{
    [Header("Propellant")]
    [SerializeField]
    private Animator _propellantAnimator;

    private int _propellantPossessedModeAnimationLayerIndex;

    protected override void Awake()
    {
        base.Awake();

        if (!_propellantAnimator)
        {
            Debug.LogError("No propellant animator was set for " + GetType() + " script of " + gameObject.name + "!");
        }
        else
        {
            _propellantPossessedModeAnimationLayerIndex = _propellantAnimator.GetLayerIndex(PossessedModeAnimationLayerName);
        }
    }

    protected override void OnPossess(PossessionPower possessingScript)
    {
        _propellantAnimator.SetLayerWeight(_propellantPossessedModeAnimationLayerIndex, 1.0f);
    }

    protected override void OnUnpossess()
    {
        _propellantAnimator.SetLayerWeight(_propellantPossessedModeAnimationLayerIndex, .0f);
    }
}
