using System.Collections.Generic;
using UnityEngine;

public interface IAnimatorEventSubscriber
{
    void NotifyEvent(string eventName);
}

public class AnimatorEventsManager : MonoBehaviour
{
    private Dictionary<string, List<IAnimatorEventSubscriber>> _subscribers = new Dictionary<string, List<IAnimatorEventSubscriber>>();

    public void Subscribe(string eventName, IAnimatorEventSubscriber subscriber)
    {
        if (_subscribers.ContainsKey(eventName))
        {
            if(!_subscribers[eventName].Contains(subscriber))
            {
                _subscribers[eventName].Add(subscriber);
            }
        }
        else
        {
            _subscribers[eventName] = new List<IAnimatorEventSubscriber> { subscriber };
        }
    }

    public void SendEvent(string eventString)
    {
        string[] eventNames = eventString.Split('/');

        foreach(string eventName in eventNames)
        {
            if (_subscribers.ContainsKey(eventName))
            {
                foreach (IAnimatorEventSubscriber subscriber in _subscribers[eventName])
                {
                    subscriber.NotifyEvent(eventName);
                }
            }
        }
    }
}

