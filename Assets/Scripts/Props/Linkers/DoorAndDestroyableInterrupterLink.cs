using System;
using UnityEngine;

[Serializable]
public struct DoorAndDestroyableInterrupterLinkSetting
{
    public DestroyableInterrupter destroyableInterrupter;
    public Door door;
    public bool openState;
}

public class DoorAndDestroyableInterrupterLink : MonoBehaviour, IDestroyableInterrupterSubscriber
{
    [Header("Links")]
    [SerializeField]
    private DoorAndDestroyableInterrupterLinkSetting[] m_doorsControl;

    private void Start()
    {
        foreach (DoorAndDestroyableInterrupterLinkSetting controlSetting in m_doorsControl)
        {
            controlSetting.destroyableInterrupter.Subscribe(this);
        }
    }

    // Methods of the IBreakableInterrupterSubscriber interface
    public void NotifyInterrupterDestroyed(DestroyableInterrupter destroyableInterrupter)
    {
        foreach (DoorAndDestroyableInterrupterLinkSetting controlSetting in m_doorsControl)
        {
            if (controlSetting.destroyableInterrupter == destroyableInterrupter)
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
