using UnityEngine;

[System.Serializable]
public struct DoorAndBreakableInterrupterLinkSetting
{
    public BreakableInterrupter breakableInterrupter;
    public Door door;
    public bool openState;
}

public class DoorAndBreakableTubeLink : MonoBehaviour, IBreakableInterrupterSubscriber
{
    [Header("Links")]
    [SerializeField]
    private DoorAndBreakableInterrupterLinkSetting[] m_doorsControl;

    private void Start()
    {
        foreach (DoorAndBreakableInterrupterLinkSetting controlSetting in m_doorsControl)
        {
            controlSetting.breakableInterrupter.Subscribe(this);
        }
    }

    // Methods of the IBreakableInterrupterSubscriber interface
    public void NotifyInterrupterBreaked(BreakableInterrupter breakableInterrupter)
    {
        foreach (DoorAndBreakableInterrupterLinkSetting controlSetting in m_doorsControl)
        {
            if (controlSetting.breakableInterrupter == breakableInterrupter)
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
