using System;
using UnityEngine;

[Serializable]
public struct DoorAndButtonLinkSetting
{
    public Button Button;
    public Door Door;
    public bool OpenState;
}

public class DoorAndButtonLink : MonoBehaviour, IButtonSubscriber
{
    [Header("Links")]
    [SerializeField]
    private DoorAndButtonLinkSetting[] _doorsControl;

    private void Start()
    {
        foreach(DoorAndButtonLinkSetting controlSetting in _doorsControl)
        {
            controlSetting.Button.Subscribe(this);
        }
    }

    // Methods of the IButtonSubscriber interface
    public void NotifyButtonPressed(Button button)
    {
        foreach(DoorAndButtonLinkSetting controlSetting in _doorsControl)
        {
            if (controlSetting.Button == button)
            {
                if (controlSetting.OpenState)
                {
                    controlSetting.Door.Open();
                }
                else
                {
                    controlSetting.Door.Close();
                }
            }
        }
    }

    public void NotifyButtonUnpressed(Button button) { }
}
