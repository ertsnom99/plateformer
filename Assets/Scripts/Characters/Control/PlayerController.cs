using UnityEngine;
using UnityEngine.InputSystem;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(Interaction))]
[RequireComponent(typeof(PossessionPower))]

public class PlayerController : CharacterController
{
    public Inputs CurrentInputs { get; protected set; }
    protected Inputs NoControlInputs = new Inputs();

    [Header("Possession")]
    [SerializeField]
    private bool _canUsePossession = false;

    private PlatformerMovement _movementScript;
    private Interaction _interaction;
    private PossessionPower _possession;

    private void Awake()
    {
        CurrentInputs = new Inputs();

        _movementScript = GetComponent<PlatformerMovement>();
        _interaction = GetComponent<Interaction>();
        _possession = GetComponent<PossessionPower>();
    }

    // Methods used to update the inputs
    #region Input action callbacks
    public void OnMove(InputAction.CallbackContext input)
    {
        Vector2 moveInput = input.ReadValue<Vector2>();

        Inputs currentInputs = CurrentInputs;
        currentInputs.Horizontal = input.ReadValue<Vector2>().x;
        currentInputs.Vertical = input.ReadValue<Vector2>().y;

        CurrentInputs = currentInputs;
    }

    public void OnChangePossessMode(InputAction.CallbackContext input)
    {
        Inputs currentInputs = CurrentInputs;
        currentInputs.PressPossess = true;

        CurrentInputs = currentInputs;
    }

    public void OnInteract(InputAction.CallbackContext input)
    {
        float interactValue = input.ReadValue<float>();

        Inputs currentInputs = CurrentInputs;

        switch (interactValue)
        {
            case 1.0f:
                currentInputs.PressInteract = true;
                break;
            case .0f:
                currentInputs.ReleaseInteract = true;
                break;
        }

        CurrentInputs = currentInputs;
    }

    public void OnHelp(InputAction.CallbackContext input)
    {
        Inputs currentInputs = CurrentInputs;
        currentInputs.PressHelp = true;

        CurrentInputs = currentInputs;
    }

    public void OnJump(InputAction.CallbackContext input)
    {
        float jumpValue = input.ReadValue<float>();

        Inputs currentInputs = CurrentInputs;

        switch (jumpValue)
        {
            case 1.0f:
                currentInputs.PressJump = true;
                break;
            case .0f:
                currentInputs.ReleaseJump = true;
                break;
        }

        CurrentInputs = currentInputs;
    }

    public void OnDash(InputAction.CallbackContext input)
    {
        float dashValue = input.ReadValue<float>();

        Inputs currentInputs = CurrentInputs;

        switch (dashValue)
        {
            case 1.0f:
                currentInputs.PressDash = true;
                break;
            case .0f:
                currentInputs.ReleaseDash = true;
                break;
        }

        CurrentInputs = currentInputs;
    }

    public void OnPower(InputAction.CallbackContext input)
    {
        float powerValue = input.ReadValue<float>();

        Inputs currentInputs = CurrentInputs;

        switch (powerValue)
        {
            case 1.0f:
                currentInputs.PressPower = true;
                currentInputs.HeldPower = true;
                break;
            case .0f:
                currentInputs.HeldPower = false;
                currentInputs.ReleasePower = true;
                break;
        }

        CurrentInputs = currentInputs;
    }
    #endregion

    //Debug.Log(Time.frameCount + ":" + input.ReadValue<float>());
    private void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > .0f)
        {
            if (ControlsEnabled())
            {
                UpdateMovement(CurrentInputs);
                UpdateInteraction(CurrentInputs);
                UpdatePossession(CurrentInputs);
            }
            else
            {
                UpdateMovement();
            }
        }
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

    private void LateUpdate()
    {
        ResetNecessaryInputs();
    }

    private void ResetNecessaryInputs()
    {
        Inputs currentInputs = CurrentInputs;

        currentInputs.PressPossess = false;
        currentInputs.PressInteract = false;
        currentInputs.ReleaseInteract = false;
        currentInputs.PressHelp = false;
        currentInputs.PressJump = false;
        currentInputs.ReleaseJump = false;
        currentInputs.PressDash = false;
        currentInputs.ReleaseDash = false;
        currentInputs.PressPower = false;
        currentInputs.ReleasePower = false;

        CurrentInputs = currentInputs;
    }

    public void SetCanUsePossession(bool canUsePossession)
    {
        _canUsePossession = canUsePossession;
    }
}
