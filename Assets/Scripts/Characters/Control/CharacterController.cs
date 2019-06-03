using UnityEngine;

public abstract class CharacterController : MonoBehaviour
{
    private bool _controlsEnabled = true;

    // Returns if the character have control of himself.
    protected virtual bool ControlsEnabled()
    {
        return _controlsEnabled;
    }

    public virtual void EnableControl(bool enable)
    {
        _controlsEnabled = enable;
    }
}
