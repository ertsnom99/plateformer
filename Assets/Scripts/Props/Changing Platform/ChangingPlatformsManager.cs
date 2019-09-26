using System;
using System.Collections.Generic;
using UnityEngine;

public class ChangingPlatformsManager : MonoBehaviour, IFadeImageSubscriber
{
    [Header("Fade out")]
    [SerializeField]
    private FadeImage _fade;

    [Header("Moving Spiked Grounds")]
    [SerializeField]
    private ChangingPlatform[] _changingPlatforms;

    private Dictionary<ChangingPlatform, bool> _changingPlatformFinishedStates = new Dictionary<ChangingPlatform, bool>();

    private void Awake()
    {
        if (!_fade)
        {
            Debug.LogError("No fade was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        foreach (ChangingPlatform changingPlatform in _changingPlatforms)
        {
            _changingPlatformFinishedStates.Add(changingPlatform, false);
        }

        _fade.Subscribe(this);
    }

    public void SetMovingSpikeFinishedState(ChangingPlatform changingPlatform, bool state)
    {
        if (Array.IndexOf(_changingPlatforms, changingPlatform) >= 0)
        {
            _changingPlatformFinishedStates[changingPlatform] = state;
        }
    }

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished() { }

    public virtual void NotifyFadeOutFinished()
    {
        foreach (ChangingPlatform changingPlatform in _changingPlatforms)
        {
            if (!_changingPlatformFinishedStates[changingPlatform])
            {
                changingPlatform.ResetPlatform();
            }
        }
    }
}
