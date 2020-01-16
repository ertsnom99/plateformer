using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class ActionEvent : UnityEvent<InputAction.CallbackContext> { }

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(Interaction))]
[RequireComponent(typeof(PossessionPower))]

public class PlayerController : CharacterController
{
    public Inputs CurrentInputs { get; protected set; }
    protected Inputs NoControlInputs = new Inputs();

    [Header("Actions name")]
    [SerializeField]
    private string _moveActionName = "Move";
    [SerializeField]
    private string _changePossessModeActionName = "ChangePossessMode";
    [SerializeField]
    private string _interactActionName = "Interact";
    [SerializeField]
    private string _helpActionName = "Help";
    [SerializeField]
    private string _jumpActionName = "Jump";
    [SerializeField]
    private string _dashActionName = "Dash";
    [SerializeField]
    private string _powerActionName = "Power";
    [SerializeField]
    private string _pauseActionName = "Pause";
    [SerializeField]
    private string _navigateActionName = "Navigate";
    [SerializeField]
    private string _submitActionName = "Submit";
    [SerializeField]
    private string _loadLevelActionName = "LoadLevel";

    [Header("Actions callback")]
    [SerializeField]
    private ActionEvent _moveCallback;
    [SerializeField]
    private ActionEvent _changePossessModeCallback;
    [SerializeField]
    private ActionEvent _interactCallback;
    [SerializeField]
    private ActionEvent _helpCallback;
    [SerializeField]
    private ActionEvent _jumpCallback;
    [SerializeField]
    private ActionEvent _dashCallback;
    [SerializeField]
    private ActionEvent _powerCallback;
    [SerializeField]
    private ActionEvent _pauseCallback;
    [SerializeField]
    private ActionEvent _navigateCallback;
    [SerializeField]
    private ActionEvent _submitCallback;
    [SerializeField]
    private ActionEvent _loadLevelCallback;

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

        BindActions();
    }

    private void BindActions()
    {
        _playerInput.actions[_moveActionName].started += OnMove;
        _playerInput.actions[_moveActionName].performed += OnMove;
        _playerInput.actions[_moveActionName].canceled += OnMove;

        _playerInput.actions[_changePossessModeActionName].started += OnChangePossessMode;
        _playerInput.actions[_changePossessModeActionName].performed += OnChangePossessMode;
        _playerInput.actions[_changePossessModeActionName].canceled += OnChangePossessMode;

        _playerInput.actions[_interactActionName].started += OnInteract;
        _playerInput.actions[_interactActionName].performed += OnInteract;
        _playerInput.actions[_interactActionName].canceled += OnInteract;

        _playerInput.actions[_helpActionName].started += OnHelp;
        _playerInput.actions[_helpActionName].performed += OnHelp;
        _playerInput.actions[_helpActionName].canceled += OnHelp;

        _playerInput.actions[_jumpActionName].started += OnJump;
        _playerInput.actions[_jumpActionName].performed += OnJump;
        _playerInput.actions[_jumpActionName].canceled += OnJump;

        _playerInput.actions[_dashActionName].started += OnDash;
        _playerInput.actions[_dashActionName].performed += OnDash;
        _playerInput.actions[_dashActionName].canceled += OnDash;

        _playerInput.actions[_powerActionName].started += OnPower;
        _playerInput.actions[_powerActionName].performed += OnPower;
        _playerInput.actions[_powerActionName].canceled += OnPower;

        _playerInput.actions[_pauseActionName].started += OnPause;
        _playerInput.actions[_pauseActionName].performed += OnPause;
        _playerInput.actions[_pauseActionName].canceled += OnPause;

        _playerInput.actions[_navigateActionName].started += OnNavigate;
        _playerInput.actions[_navigateActionName].performed += OnNavigate;
        _playerInput.actions[_navigateActionName].canceled += OnNavigate;

        _playerInput.actions[_submitActionName].started += OnSubmit;
        _playerInput.actions[_submitActionName].performed += OnSubmit;
        _playerInput.actions[_submitActionName].canceled += OnSubmit;

        _playerInput.actions[_loadLevelActionName].started += OnLoadLevel;
        _playerInput.actions[_loadLevelActionName].performed += OnLoadLevel;
        _playerInput.actions[_loadLevelActionName].canceled += OnLoadLevel;
    }

    // Methods used to update the inputs
    #region Input action callbacks
    private void OnMove(InputAction.CallbackContext input)
    {
        Vector2 moveInput = input.ReadValue<Vector2>();

        Inputs currentInputs = CurrentInputs;
        currentInputs.Horizontal = input.ReadValue<Vector2>().x;
        currentInputs.Vertical = input.ReadValue<Vector2>().y;

        CurrentInputs = currentInputs;

        _moveCallback.Invoke(input);
    }

    private void OnChangePossessMode(InputAction.CallbackContext input)
    {
        Inputs currentInputs = CurrentInputs;
        currentInputs.PressPossess = true;

        CurrentInputs = currentInputs;

        _changePossessModeCallback.Invoke(input);
    }

    private void OnInteract(InputAction.CallbackContext input)
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

        _interactCallback.Invoke(input);
    }

    private void OnHelp(InputAction.CallbackContext input)
    {
        Inputs currentInputs = CurrentInputs;
        currentInputs.PressHelp = true;

        CurrentInputs = currentInputs;

        _helpCallback.Invoke(input);
    }

    private void OnJump(InputAction.CallbackContext input)
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

        _jumpCallback.Invoke(input);
    }

    private void OnDash(InputAction.CallbackContext input)
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

        _dashCallback.Invoke(input);
    }

    private void OnPower(InputAction.CallbackContext input)
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

        _powerCallback.Invoke(input);
    }

    private void OnPause(InputAction.CallbackContext input)
    {
        _pauseCallback.Invoke(input);
    }

    private void OnNavigate(InputAction.CallbackContext input)
    {
        _navigateCallback.Invoke(input);
    }

    private void OnSubmit(InputAction.CallbackContext input)
    {
        _submitCallback.Invoke(input);
    }

    private void OnLoadLevel(InputAction.CallbackContext input)
    {
        _loadLevelCallback.Invoke(input);
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
