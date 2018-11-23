using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSubscribable<T> : MonoBehaviour
{
    protected List<T> m_subscribers = new List<T>();
    
    public virtual void Subscribe(T subscriber)
    {
        m_subscribers.Add(subscriber);
    }

    public virtual void Unsubscribe(T subscriber)
    {
        m_subscribers.Remove(subscriber);
    }
}
