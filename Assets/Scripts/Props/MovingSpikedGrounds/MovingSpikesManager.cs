using System;
using System.Collections.Generic;
using UnityEngine;

public class MovingSpikesManager : MonoBehaviour, IFadeImageSubscriber
{
    [Header("Fade out")]
    [SerializeField]
    private FadeImage _fade;

    [Header("Moving Spiked Grounds")]
    [SerializeField]
    private MovingSpike[] _movingSpikes;

    private Dictionary<MovingSpike, bool> _movingSpikeFinishedStates = new Dictionary<MovingSpike, bool>();

    private void Awake()
    {
        if (!_fade)
        {
            Debug.LogError("No fade was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        foreach (MovingSpike movingSpike in _movingSpikes)
        {
            _movingSpikeFinishedStates.Add(movingSpike, false);
        }

        _fade.Subscribe(this);
    }

    public void SetMovingSpikeFinishedState(MovingSpike movingSpike, bool state)
    {
        if (Array.IndexOf(_movingSpikes, movingSpike) >= 0)
        {
            _movingSpikeFinishedStates[movingSpike] = state;
        }
    }

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished() { }

    public virtual void NotifyFadeOutFinished()
    {
        foreach(MovingSpike movingSpike in _movingSpikes)
        {
            if (!_movingSpikeFinishedStates[movingSpike])
            {
                movingSpike.ResetMovement();
            }
            else
            {
                movingSpike.SetToEndPosition();
            }
        }
    }
}
