using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    public Controller Controller { get; private set; }

    public virtual void SetController(Controller controller)
    {
        Controller = controller;
    }

    public virtual void UpdateWithInputs(Inputs inputs) { }
}
