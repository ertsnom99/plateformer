using System;
using UnityEngine;

[Serializable]
public struct DoorAndButtonLinkSetting
{
    public Button button;
    public Door door;
    public bool openState;
}

public class DoorAndButtonLink : MonoBehaviour, IButtonSubscriber
{
    [Header("Links")]
    [SerializeField]
    private DoorAndButtonLinkSetting[] m_doorsControl;

    private void Start()
    {
        foreach(DoorAndButtonLinkSetting controlSetting in m_doorsControl)
        {
            controlSetting.button.Subscribe(this);
        }
    }

    // Methods of the IButtonSubscriber interface
    public void NotifyButtonPressed(Button button)
    {
        foreach(DoorAndButtonLinkSetting controlSetting in m_doorsControl)
        {
            if (controlSetting.button == button)
            {
                if (controlSetting.openState)
                {
                    controlSetting.door.Open();
                }
                else
                {
                    controlSetting.door.Close();
                }
            }
        }
    }
}
