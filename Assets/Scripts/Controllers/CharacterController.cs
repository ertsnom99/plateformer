using UnityEngine;

public abstract class CharacterController : MonoBehaviour
{
    [SerializeField]
    protected Character _controlledCharacter; 

    private bool _controlsEnabled = true;
    protected Inputs NoControlInputs = new Inputs();

    protected virtual void Awake()
    {
        if (!SetControlledCharacter(_controlledCharacter))
        {
            _controlledCharacter = null;
        }
    }
    
    public bool SetControlledCharacter(Character character)
    {
        // Can't take control of a character already controlled!
        if (character?.Controller)
        {
            return false;
        }

        if (!character && _controlledCharacter)
        {
            _controlledCharacter.UpdateWithInputs(NoControlInputs);
            _controlledCharacter.SetController(null);
        }
        else if (character)
        {
            character.SetController(this);
        }

        _controlledCharacter = character;
        return true;
    }

    public Character GetControlledCharacter()
    {
        return _controlledCharacter;
    }

    // Returns if the character have control of himself.
    public virtual bool ControlsEnabled()
    {
        return _controlsEnabled;
    }

    public virtual void EnableControl(bool enable)
    {
        _controlsEnabled = enable;

        if (!enable && _controlledCharacter)
        {
            _controlledCharacter.UpdateWithInputs(NoControlInputs);
        }
    }
}
