using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    [Header("Pawn")]
    [SerializeField]
    protected Pawn _controlledPawn; 

    private bool _controlsEnabled = true;
    protected Inputs NoControlInputs = new Inputs();

    protected virtual void Start()
    {
        if (!SetControlledPawn(_controlledPawn))
        {
            _controlledPawn = null;
        }
    }
    
    public bool SetControlledPawn(Pawn pawn)
    {
        // Can't take control of a pawn already controlled!
        if (pawn?.Controller)
        {
            return false;
        }

        // Free any pawn already controlled
        if (_controlledPawn)
        {
            _controlledPawn.UpdateWithInputs(NoControlInputs);
            _controlledPawn.SetController(null);
        }

        if (pawn)
        {
            pawn.SetController(this);
        }

        _controlledPawn = pawn;
        return true;
    }

    public Pawn GetControlledPawn()
    {
        return _controlledPawn;
    }

    // Returns if the pawn have control of himself.
    public virtual bool ControlsEnabled()
    {
        return _controlsEnabled;
    }

    public virtual void EnableControl(bool enable)
    {
        _controlsEnabled = enable;

        if (!enable && _controlledPawn)
        {
            _controlledPawn.UpdateWithInputs(NoControlInputs);
        }
    }
}
