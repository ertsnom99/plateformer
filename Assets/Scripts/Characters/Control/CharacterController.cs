using UnityEngine;

public abstract class CharacterController : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField]
    protected bool UseKeyboard = false;

    protected Inputs NoControlInputs = new Inputs();

    private bool _controlsEnabled = true;

    // Returns if the character have control of himself.
    protected virtual bool ControlsEnabled()
    {
        return _controlsEnabled;
    }

    public virtual void EnableControl(bool enable)
    {
        _controlsEnabled = enable;

        UpdateMovement(NoControlInputs);
    }

    public void SetKeyboardUse(bool useKeyboard)
    {
        UseKeyboard = useKeyboard;
    }

    protected abstract Inputs FetchInputs();

    protected abstract void UpdateMovement(Inputs inputs);
}
