using System;
using UnityEngine;

[Serializable]
public struct MovingSpikeAndTriggerLinkSetting
{
    public TriggerZone TriggerZone;
    public MovingSpike MovingSpike;
    public bool StartMovingSpike;
    public bool ChangeMovingSpikesStateInManager;
    public bool MovingSpikesStateInManager;
}

public class MovingSpikeAndTriggerLink : MonoBehaviour, ITriggerZoneSubscriber
{
    [SerializeField]
    private MovingSpikesManager _movingSpikesManager;

    [Header("Links")]
    [SerializeField]
    private MovingSpikeAndTriggerLinkSetting[] _movingSpikeControl;

    private void Start()
    {
        foreach (MovingSpikeAndTriggerLinkSetting controlSetting in _movingSpikeControl)
        {
            controlSetting.TriggerZone.Subscribe(this);
        }
    }

    // Methods of the ITriggerZoneSubscriber interface
    public void NotifyTriggerEntered(TriggerZone triggerZone)
    {
        foreach (MovingSpikeAndTriggerLinkSetting controlSetting in _movingSpikeControl)
        {
            if (controlSetting.TriggerZone == triggerZone)
            {
                controlSetting.MovingSpike.StartMoving();

                if (controlSetting.ChangeMovingSpikesStateInManager)
                {
                    _movingSpikesManager.SetMovingSpikeFinishedState(controlSetting.MovingSpike, controlSetting.MovingSpikesStateInManager);
                }
            }
        }
    }
}
