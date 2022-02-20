using UnityEngine;
using UnityEngine.InputSystem;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlayerInput))]

public class PlayerController : CharacterController
{
    private InputDevice activeGamepad;

    // Action map names
    private readonly string _gamePlayActionMapName = "GamePlay";
    private readonly string _moveActionName = "Move";
    private readonly string _jumpActionName = "Jump";
    private readonly string _dashActionName = "Dash";
    private readonly string _interactActionName = "Interact";
    private readonly string _possessActionName = "Possess";
    private readonly string _powerActionName = "Power";
    private readonly string _helpActionName = "Help";
    private readonly string _pauseActionName = "Pause";

    // Actions
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _dashAction;
    private InputAction _interactAction;
    private InputAction _possessAction;
    private InputAction _powerAction;
    private InputAction _helpAction;
    private InputAction _pauseAction;

    public Inputs CurrentInputs { get; protected set; }

    private PlayerInput _playerInput;

    protected override void Awake()
    {
        base.Awake();

        _playerInput = GetComponent<PlayerInput>();

        EnableFirstGamepad();
        InputSystem.onDeviceChange += UpdateGamepads;

        //EnableActionMaps();

        CurrentInputs = new Inputs();
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

    /*private void EnableActionMaps()
    {
        _playerInput.actions.FindActionMap(GamePlayActionMapName).Enable();
        _playerInput.actions.FindActionMap(UIActionMapName).Disable();
    }*/

    private void SaveActions()
    {
        _moveAction = _playerInput.actions[_moveActionName];
        _jumpAction = _playerInput.actions[_jumpActionName];
        _dashAction = _playerInput.actions[_dashActionName];
        _interactAction = _playerInput.actions[_interactActionName];
        _possessAction = _playerInput.actions[_possessActionName];
        _powerAction = _playerInput.actions[_powerActionName];
        _helpAction = _playerInput.actions[_helpActionName];
        _pauseAction = _playerInput.actions[_pauseActionName];
    }
    
    // Methods used to update the inputs
    #region Inputs update
    private void UpdateCurrentInputs()
    {
        UpdateMoveInputs();
        UpdateJumpInput();
        UpdateDashInput();
        UpdateInteractInput();
        UpdatePossessInput();
        UpdatePowerInput();
        UpdateHelpInput();
        UpdatePauseInput();
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

    private void UpdateInteractInput()
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

    private void UpdatePossessInput()
    {
        Inputs currentInputs = CurrentInputs;

        if (_possessAction.triggered && _possessAction.ReadValue<float>() == 1.0f)
        {
            currentInputs.PressPossess = true;
        }

        CurrentInputs = currentInputs;
    }

    private void UpdatePowerInput()
    {
        Inputs currentInputs = CurrentInputs;

        if (_powerAction.triggered && _powerAction.ReadValue<float>() == 1.0f)
        {
            currentInputs.PressPower = true;
            currentInputs.HeldPower = true;
        }
        else if (_powerAction.triggered && _powerAction.ReadValue<float>() == .0f)
        {
            currentInputs.ReleasePower = true;
            currentInputs.HeldPower = false;
        }

        CurrentInputs = currentInputs;
    }

    private void UpdateHelpInput()
    {
        Inputs currentInputs = CurrentInputs;

        if (_helpAction.triggered && _helpAction.ReadValue<float>() == 1.0f)
        {
            currentInputs.PressHelp = true;
        }

        CurrentInputs = currentInputs;
    }

    private void UpdatePauseInput()
    {
        Inputs currentInputs = CurrentInputs;

        if (_pauseAction.triggered && _pauseAction.ReadValue<float>() == 1.0f)
        {
            currentInputs.PressPause = true;
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
            
            // TODO: Handle pause input

            if (_controlledCharacter && ControlsEnabled())
            {
                _controlledCharacter.UpdateWithInputs(CurrentInputs);
            }
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
        currentInputs.PressPower = false;
        currentInputs.ReleasePower = false;
        currentInputs.PressHelp = false;
        currentInputs.PressPause = false;

        CurrentInputs = currentInputs;
    }
}
