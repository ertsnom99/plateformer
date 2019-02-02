using UnityEngine;

public abstract class CharacterControl : MonoBehaviour
{
    protected Inputs noControlInputs;

    public bool ControlsEnabled { get; protected set; }

    protected virtual void Awake()
    {
        noControlInputs = new Inputs();
        ControlsEnabled = true;
    }

    protected virtual bool ControlsCharacter()
    {
        return ControlsEnabled;
    }

    protected abstract void UpdateMovement(Inputs inputs);

    public void EnableControl(bool enable)
    {
        if (!enable)
        {
            UpdateMovement(noControlInputs);
        }

        ControlsEnabled = enable;
    }
}
