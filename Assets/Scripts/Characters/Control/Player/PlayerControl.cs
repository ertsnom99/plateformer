using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]

public class PlayerControl : CharacterControl
{
    [Header("Controls")]
    [SerializeField]
    private bool _useKeyboard = false;

    private PlatformerMovement _movementScript;

    protected override void Awake()
    {
        base.Awake();

        _movementScript = GetComponent<PlatformerMovement>();
    }

    private void Update()
    {
        // Only update when time isn't stop
        if (Time.deltaTime > .0f && ControlsCharacter())
        {
            // Get the inputs used during this frame
            Inputs inputs = FetchInputs();

            UpdateMovement(inputs);
        }
    }

    private Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();

        if (_useKeyboard)
        {
            // Inputs from the keyboard
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Jump = Input.GetButtonDown("Jump");
            inputs.ReleaseJump = Input.GetButtonUp("Jump");
            inputs.Dash = Input.GetButtonDown("Dash");
            inputs.ReleaseDash = Input.GetButtonUp("Dash");
        }
        else
        {
            // TODO: Create inputs specific to the controler
            // Inputs from the controler
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Jump = Input.GetButtonDown("Jump");
            inputs.ReleaseJump = Input.GetButtonUp("Jump");
            inputs.Dash = Input.GetButtonDown("Dash");
            inputs.ReleaseDash = Input.GetButtonUp("Dash");
        }

        return inputs;
    }

    public void SetKeyboardUse(bool useKeyboard)
    {
        _useKeyboard = useKeyboard;
    }

    protected override void UpdateMovement(Inputs inputs)
    {
        _movementScript.SetInputs(inputs);
    }
}
