using UnityEngine;

// This script requires those components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]

public class MobileCannonCharacter : PossessablePawn
{
    [Header("Cannon")]
    [SerializeField]
    private ChargeCannon _cannon;

    private PlatformerMovement _platformerMovementScript;

    protected override void Awake()
    {
        base.Awake();

        _platformerMovementScript = GetComponent<PlatformerMovement>();
        
        SetIsLookingForwardDelegate(_platformerMovementScript.IsLookingForward);

        if (!_cannon)
        {
            Debug.LogError("No cannon was set for " + GetType() + " script of " + gameObject.name + "!");
        }
    }

    public override void UpdateWithInputs(Inputs inputs)
    {
        UpdateMovement(inputs);
        UpdateCannon(inputs);
        base.UpdateWithInputs(inputs);
    }

    protected override bool CanUnpossess()
    {
        return !_cannon.IsCharging;
    }

    private void UpdateMovement(Inputs inputs)
    {
        _platformerMovementScript.SetInputs(inputs);
        _platformerMovementScript.UpdateMovement();
    }

    private void UpdateCannon(Inputs inputs)
    {
        if (inputs.PressPower && !_cannon.IsCharging)
        {
            _cannon.StartCharge();
        }
        else if (inputs.ReleasePower && _cannon.IsCharging)
        {
            _cannon.StopCharge();
        }
    }

    protected override void OnPossess()
    {
        if (_cannon)
        {
            _cannon.UpdatePossessedVisual(true);
        }
    }

    protected override void OnUnpossess()
    {
        if (_cannon)
        {
            _cannon.UpdatePossessedVisual(false);
        }
    }
}
