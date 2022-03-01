using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(FlyingMovement))]

public class FlyingCharacter : PossessablePawn
{
    private FlyingMovement _movementScript;

    protected override void Awake()
    {
        base.Awake();

        _movementScript = GetComponent<FlyingMovement>();
    }

    public override void UpdateWithInputs(Inputs inputs)
    {
        UpdateMovement(inputs);
        base.UpdateWithInputs(inputs);
    }

    private void UpdateMovement(Inputs inputs)
    {
        _movementScript.SetInputs(inputs);
    }
}
