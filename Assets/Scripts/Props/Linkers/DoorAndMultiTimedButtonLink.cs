using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DoorAndMultiTimedButtonLinkSetting
{
    public TimedButton[] Buttons;
    public Door Door;
    public bool OpenState;
}

public class DoorAndMultiTimedButtonLink : MonoBehaviour, IButtonSubscriber
{
    [Header("Links")]
    [SerializeField]
    private DoorAndMultiTimedButtonLinkSetting[] _doorsControl;

    private Dictionary<DoorAndMultiTimedButtonLinkSetting, int> _pressedButtons = new Dictionary<DoorAndMultiTimedButtonLinkSetting, int>();

    private void Start()
    {
        foreach (DoorAndMultiTimedButtonLinkSetting controlSetting in _doorsControl)
        {
            foreach(TimedButton button in controlSetting.Buttons)
            {
                button.Subscribe(this);
            }

            _pressedButtons.Add(controlSetting, 0);
        }
    }

    // Methods of the IButtonSubscriber interface
    public void NotifyButtonPressed(Button button)
    {
        foreach (DoorAndMultiTimedButtonLinkSetting controlSetting in _doorsControl)
        {
            foreach (TimedButton settingButton in controlSetting.Buttons)
            {
                if (settingButton == button)
                {
                    _pressedButtons[controlSetting]++;
                }
            }

            if (_pressedButtons[controlSetting] >= controlSetting.Buttons.Length)
            {
                if (controlSetting.OpenState)
                {
                    controlSetting.Door.Open();
                }
                else
                {
                    controlSetting.Door.Close();
                }
                
                foreach (TimedButton settingButton in controlSetting.Buttons)
                {
                    settingButton.UnpressAfterPress(false);
                }

            }
        }
    }

    public void NotifyButtonUnpressed(Button button)
    {
        foreach (DoorAndMultiTimedButtonLinkSetting controlSetting in _doorsControl)
        {
            foreach (TimedButton settingButton in controlSetting.Buttons)
            {
                if (settingButton == button)
                {
                    _pressedButtons[controlSetting]--;
                }
            }
        }
    }
}
