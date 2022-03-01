using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(Interaction))]
[RequireComponent(typeof(PossessionPower))]

public class PlayerCharacter : Pawn
{
    [Header("Possession")]
    [SerializeField]
    private bool _canUsePossession = false;

    private Inputs _noControlInputs = new Inputs();

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private PlatformerMovement _movementScript;
    private Interaction _interactionScript;
    private PossessionPower _possessionScript;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _movementScript = GetComponent<PlatformerMovement>();
        _interactionScript = GetComponent<Interaction>();
        _possessionScript = GetComponent<PossessionPower>();

        _possessionScript.SetPossessingCharacter(this);
        _possessionScript.SetShowCharacterDelegate(ShowCharacter);
        _possessionScript.SetChangeOrientationDelegate(_movementScript.ChangeOrientation);
    }

    private void ShowCharacter(bool show)
    {
        _spriteRenderer.enabled = show;
        _collider.enabled = show;
        _movementScript.enabled = show;
    }

    public override void UpdateWithInputs(Inputs inputs)
    {
        UpdateMovement(inputs);
        UpdateInteraction(inputs);
        UpdatePossession(inputs);
    }

    private void UpdateMovement(Inputs inputs)
    {
        if (!_interactionScript.Interacting)
        {
            _movementScript.SetInputs(inputs);
        }
        else
        {
            _movementScript.SetInputs(_noControlInputs);
        }

        _movementScript.UpdateMovement();
    }

    private void UpdateInteraction(Inputs inputs)
    {
        if (inputs.PressInteract)
        {
            _interactionScript.BeginInteraction();
        }
        else if (inputs.ReleaseInteract && _interactionScript.Interacting)
        {
            _interactionScript.StopInteraction();
        }
    }

    private void UpdatePossession(Inputs inputs)
    {
        if (inputs.PressPossess && !_interactionScript.Interacting)
        {
            _possessionScript.ChangePossessionMode(_canUsePossession && !_possessionScript.InPossessionMode);
        }
    }

    public void SetCanUsePossession(bool canUsePossession)
    {
        _canUsePossession = canUsePossession;
    }
}
