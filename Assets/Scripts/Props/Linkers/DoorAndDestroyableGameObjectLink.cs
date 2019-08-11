using System;
using UnityEngine;

[Serializable]
public struct DoorAndDestroyableGameObjectLinkSetting
{
    public DestroyableGameObject DestroyableGameObject;
    public Door Door;
    public bool OpenState;
}

public class DoorAndDestroyableGameObjectLink : MonoBehaviour, IDestroyableGameObjectSubscriber
{
    [Header("Links")]
    [SerializeField]
    private DoorAndDestroyableGameObjectLinkSetting[] _doorsControl;

    private void Start()
    {
        foreach (DoorAndDestroyableGameObjectLinkSetting controlSetting in _doorsControl)
        {
            controlSetting.DestroyableGameObject.Subscribe(this);
        }
    }

    // Methods of the IDestroyableGameObjectSubscriber interface
    public void NotifyGameObjectDestroyed(DestroyableGameObject destroyableGameObject)
    {
        foreach (DoorAndDestroyableGameObjectLinkSetting controlSetting in _doorsControl)
        {
            if (controlSetting.DestroyableGameObject == destroyableGameObject)
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
