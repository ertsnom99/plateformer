using System.Collections.Generic;
using UnityEngine;

public interface IAnimatorEventSubscriber
{
    void NotifyEvent(string eventName);
}

public class AnimatorEventsManager : MonoBehaviour
{
    private Dictionary<string, List<IAnimatorEventSubscriber>> m_subscribers = new Dictionary<string, List<IAnimatorEventSubscriber>>();

    public void Subscribe(string eventName, IAnimatorEventSubscriber subscriber)
    {
        if (m_subscribers.ContainsKey(eventName))
        {
            if(!m_subscribers[eventName].Contains(subscriber))
            {
                m_subscribers[eventName].Add(subscriber);
            }
        }
        else
        {
            m_subscribers[eventName] = new List<IAnimatorEventSubscriber> { subscriber };
        }
    }

    public void SendEvent(string eventString)
    {
        string[] eventNames = eventString.Split('/');

        foreach(string eventName in eventNames)
        {
            if (m_subscribers.ContainsKey(eventName))
            {
                foreach (IAnimatorEventSubscriber subscriber in m_subscribers[eventName])
                {
                    subscriber.NotifyEvent(eventName);
                }
            }
        }
    }
}

