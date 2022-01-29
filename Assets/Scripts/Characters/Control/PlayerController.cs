using UnityEngine;
using UnityEngine.InputSystem;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(Interaction))]
[RequireComponent(typeof(PossessionPower))]

public class PlayerController : CharacterController
{
    private InputDevice activeGamepad;

    public Inputs CurrentInputs { get; protected set; }
    protected Inputs NoControlInputs = new Inputs();

    [Header("Actions name")]
    [SerializeField]
    private string _moveActionName = "Move";
    [SerializeField]
    private string _jumpActionName = "Jump";
    [SerializeField]
    private string _dashActionName = "Dash";
    [SerializeField]
    private string _interactActionName = "Interact";
    [SerializeField]
    private string _changePossessModeActionName = "ChangePossessMode";

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _interactAction;
    private InputAction _changePossessModeAction;

    [Header("Possession")]
    [SerializeField]
    private bool _canUsePossession = false;

    private PlayerInput _playerInput;
    private PlatformerMovement _movementScript;
    private Interaction _interaction;
    private PossessionPower _possession;

    private void Awake()
    {
        CurrentInputs = new Inputs();

        _playerInput = GetComponent<PlayerInput>();
        _movementScript = GetComponent<PlatformerMovement>();
        _interaction = GetComponent<Interaction>();
        _possession = GetComponent<PossessionPower>();

        EnableFirstGamepad();
        InputSystem.onDeviceChange += UpdateGamepads;

        SaveActions();
    }

    private void EnableFirstGamepad()
    {
        activeGamepad = null;

        foreach (InputDevice device in _playerInput.devices)
        {
            if (!(device is Gamepad))
            {
                continue;
            }

            if (activeGamepad == null)
            {
                InputSystem.EnableDevice(device);
                activeGamepad = device;
            }
            else
            {
                InputSystem.DisableDevice(device);
            }
        }
    }

    private void UpdateGamepads(InputDevice device, InputDeviceChange change)
    {
        if (!(device is Gamepad))
        {
            return;
        }

        switch (change)
        {
            case InputDeviceChange.Added:
                if (activeGamepad != null)
                {
                    InputSystem.DisableDevice(device);
                }
                else
                {
                    activeGamepad = device;
                }

                break;

            case InputDeviceChange.Removed:
                if (activeGamepad == device)
                {
                    EnableFirstGamepad();
                }

                break;
        }
    }

    private void SaveActions()
    {
        _moveAction = _playerInput.actions[_moveActionName];
        _jumpAction = _playerInput.actions[_jumpActionName];
        _dashAction = _playerInput.actions[_dashActionName];
        _interactAction = _playerInput.actions[_interactActionName];
        _changePossessModeAction = _playerInput.actions[_changePossessModeActionName];
    }

    // Methods used to update the inputs
    #region Inputs update
    private void UpdateCurrentInputs()
    {
        UpdateMoveInputs();
        UpdateJumpInput();
        UpdateDashInput();
        UpdateInteract();
        UpdateChangePossessModeInput();
    }

    private void UpdateMoveInputs()
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();

        Inputs currentInputs = CurrentInputs;
        currentInputs.Horizontal = moveInput.x;
        currentInputs.Vertical = moveInput.y;

        CurrentInputs = currentInputs;
    }

    private void UpdateJumpInput()
    {
        Inputs currentInputs = CurrentInputs;

        if (_jumpAction.triggered && _jumpAction.ReadValue<float>() == 1.0f)
        {
            currentInputs.PressJump = true;
        }
        else if (_jumpAction.triggered && _jumpAction.ReadValue<float>() == .0f)
        {
            currentInputs.ReleaseJump = true;
        }

        CurrentInputs = currentInputs;
    }

    private void UpdateDashInput()
    {
        Inputs currentInputs = CurrentInputs;

        if (_dashAction.triggered && _dashAction.ReadValue<float>() == 1.0f)
        {
            currentInputs.PressDash = true;
        }
        else if (_dashAction.triggered && _dashAction.ReadValue<float>() == .0f)
        {
            currentInputs.ReleaseDash = true;
        }

        CurrentInputs = currentInputs;
    }

    private void UpdateInteract()
    {
        Inputs currentInputs = CurrentInputs;

        if (_interactAction.triggered && _interactAction.ReadValue<float>() == 1.0f)
        {
            currentInputs.PressInteract = true;
        }
        else if (_interactAction.triggered && _interactAction.ReadValue<float>() == .0f)
        {
            currentInputs.ReleaseInteract = true;
        }

        CurrentInputs = currentInputs;
    }

    private void UpdateChangePossessModeInput()
    {
        Inputs currentInputs = CurrentInputs;

        if (_changePossessModeAction.triggered && _changePossessModeAction.ReadValue<float>() == 1.0f)
        {
            currentInputs.PressPossess = true;
        }

        CurrentInputs = currentInputs;
    }
    #endregion

    private void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > .0f)
        {
            UpdateCurrentInputs();

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

        currentInputs.PressJump = false;
        currentInputs.ReleaseJump = false;
        currentInputs.PressDash = false;
        currentInputs.ReleaseDash = false;
        currentInputs.PressInteract = false;
        currentInputs.ReleaseInteract = false;
        currentInputs.PressPossess = false;

        CurrentInputs = currentInputs;
    }

    public void SetCanUsePossession(bool canUsePossession)
    {
        _canUsePossession = canUsePossession;
    }
}
