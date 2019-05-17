using System;
using UnityEngine;

[Serializable]
public struct DoorAndTriggerZoneLinkSetting
{
    public TriggerZone TriggerZone;
    public Door Door;
    public bool OpenState;
}

public class DoorAndTriggerLink : MonoBehaviour, ITriggerZoneSubscriber
{
    [Header("Links")]
    [SerializeField]
    private DoorAndTriggerZoneLinkSetting[] _doorsControl;

    private void Start()
    {
        foreach (DoorAndTriggerZoneLinkSetting controlSetting in _doorsControl)
        {
            controlSetting.TriggerZone.Subscribe(this);
        }
    }

    // Methods of the ITriggerZoneSubscriber interface
    public void NotifyTriggerEntered(TriggerZone triggerZone)
    {
        foreach (DoorAndTriggerZoneLinkSetting controlSetting in _doorsControl)
        {
            if (controlSetting.TriggerZone == triggerZone)
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
}
