using System;
using UnityEngine;

[Serializable]
public struct DoorAndDestroyableInterrupterLinkSetting
{
    public DestroyableInterrupter DestroyableInterrupter;
    public Door Door;
    public bool OpenState;
}

public class DoorAndDestroyableInterrupterLink : MonoBehaviour, IDestroyableInterrupterSubscriber
{
    [Header("Links")]
    [SerializeField]
    private DoorAndDestroyableInterrupterLinkSetting[] _doorsControl;

    private void Start()
    {
        foreach (DoorAndDestroyableInterrupterLinkSetting controlSetting in _doorsControl)
        {
            controlSetting.DestroyableInterrupter.Subscribe(this);
        }
    }

    // Methods of the IBreakableInterrupterSubscriber interface
    public void NotifyInterrupterDestroyed(DestroyableInterrupter destroyableInterrupter)
    {
        foreach (DoorAndDestroyableInterrupterLinkSetting controlSetting in _doorsControl)
        {
            if (controlSetting.DestroyableInterrupter == destroyableInterrupter)
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
