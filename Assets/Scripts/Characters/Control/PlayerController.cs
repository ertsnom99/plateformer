using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(Interaction))]
[RequireComponent(typeof(PossessionPower))]

public class PlayerController : CharacterController
{
    [Header("Controls")]
    [SerializeField]
    protected bool UseKeyboard = false;

    protected Inputs NoControlInputs = new Inputs();

    [Header("Possession")]
    [SerializeField]
    private bool _canUsePossession = false;

    private PlatformerMovement _movementScript;
    private Interaction _interaction;
    private PossessionPower _possession;

    private void Awake()
    {
        _movementScript = GetComponent<PlatformerMovement>();
        _interaction = GetComponent<Interaction>();
        _possession = GetComponent<PossessionPower>();
    }

    private void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > .0f)
        {
            if (ControlsEnabled())
            {
                // Get the inputs used during this frame
                Inputs inputs = FetchInputs();

                UpdateMovement(inputs);
                UpdateInteraction(inputs);
                UpdatePossession(inputs);
            }
            else
            {
                UpdateMovement();
            }
        }
    }

    private Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();

        if (UseKeyboard)
        {
            // Inputs from the keyboard
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Jump = Input.GetButtonDown("Jump");
            inputs.ReleaseJump = Input.GetButtonUp("Jump");
            inputs.Dash = Input.GetButtonDown("Dash");
            inputs.ReleaseDash = Input.GetButtonUp("Dash");
            inputs.Interact = Input.GetButtonDown("Interact");
            inputs.ReleaseInteract = Input.GetButtonUp("Interact");
            inputs.Possess = Input.GetButtonDown("Possess");
        }
        else
        {
            // Inputs from the controler
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Jump = Input.GetButtonDown("Jump");
            inputs.ReleaseJump = Input.GetButtonUp("Jump");
            inputs.Dash = Input.GetButtonDown("Dash");
            inputs.ReleaseDash = Input.GetButtonUp("Dash");
            inputs.Interact = Input.GetButtonDown("Interact");
            inputs.ReleaseInteract = Input.GetButtonUp("Interact");
            inputs.Possess = Input.GetButtonDown("Possess");
        }

        return inputs;
    }

    private void UpdateMovement(Inputs inputs)
    {
        if (!_interaction.Interacting)
        {
            _movementScript.SetInputs(inputs);
        }
        else
        {
            _movementScript.SetInputs(NoControlInputs);
        }

        _movementScript.UpdateMovement();
    }

    private void UpdateMovement()
    {
        _movementScript.UpdateMovement();
    }

    private void UpdateInteraction(Inputs inputs)
    {
        if (inputs.Interact)
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
        if (inputs.Possess && !_interaction.Interacting)
        {
            _possession.ChangePossessionMode(_canUsePossession && !_possession.InPossessionMode);
        }
    }

    public void SetKeyboardUse(bool useKeyboard)
    {
        UseKeyboard = useKeyboard;
    }

    public void SetCanUsePossession(bool canUsePossession)
    {
        _canUsePossession = canUsePossession;
    }
}
