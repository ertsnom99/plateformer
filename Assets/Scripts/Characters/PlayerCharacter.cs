using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(Interaction))]
[RequireComponent(typeof(PossessionPower))]

public class PlayerCharacter : Character
{
    [Header("Possession")]
    [SerializeField]
    private bool _canUsePossession = false;

    private Inputs _noControlInputs = new Inputs();

    private PlatformerMovement _movementScript;
    private Interaction _interaction;
    private PossessionPower _possession;

    private void Awake()
    {
        _movementScript = GetComponent<PlatformerMovement>();
        _interaction = GetComponent<Interaction>();
        _possession = GetComponent<PossessionPower>();
    }

    public override void UpdateWithInputs(Inputs inputs)
    {
        UpdateMovement(inputs);
        UpdateInteraction(inputs);
        UpdatePossession(inputs);
    }

    private void UpdateMovement(Inputs inputs)
    {
        if (!_interaction.Interacting)
        {
            _movementScript.SetInputs(inputs);
        }
        else
        {
            _movementScript.SetInputs(_noControlInputs);
        }

        _movementScript.UpdateMovement();
    }

    private void UpdateMovement()
    {
        _movementScript.UpdateMovement();
    }

    private void UpdateInteraction(Inputs inputs)
    {
        if (inputs.PressInteract)
        {
            _interaction.BeginInteraction();
        }
        else if (inputs.ReleaseInteract && _interaction.Interacting)
        {
            _interaction.StopInteraction();
        }
    }

    private void UpdatePossession(Inputs inputs)
    {
        if (inputs.PressPossess && !_interaction.Interacting)
        {
            _possession.ChangePossessionMode(_canUsePossession && !_possession.InPossessionMode);
        }
    }

    public void SetCanUsePossession(bool canUsePossession)
    {
        _canUsePossession = canUsePossession;
    }
}
