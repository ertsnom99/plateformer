using UnityEngine;

[System.Serializable]
public struct DoorAndTriggerZoneLinkSetting
{
    public TriggerZone triggerZone;
    public Door door;
    public bool openState;
}

public class DoorAndTriggerLink : MonoBehaviour, ITriggerZoneSubscriber
{
    [Header("Links")]
    [SerializeField]
    private DoorAndTriggerZoneLinkSetting[] m_doorsControl;

    private void Start()
    {
        foreach (DoorAndTriggerZoneLinkSetting controlSetting in m_doorsControl)
        {
            controlSetting.triggerZone.Subscribe(this);
        }
    }

    // Methods of the ITriggerZoneSubscriber interface
    public void NotifyTriggerEntered(TriggerZone triggerZone)
    {
        foreach (DoorAndTriggerZoneLinkSetting controlSetting in m_doorsControl)
        {
            if (controlSetting.triggerZone == triggerZone)
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
