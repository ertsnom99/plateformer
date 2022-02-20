using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public CharacterController Controller { get; private set; }

    public void SetController(CharacterController controller)
    {
        Controller = controller;
    }

    public virtual void UpdateWithInputs(Inputs inputs) { }
}
