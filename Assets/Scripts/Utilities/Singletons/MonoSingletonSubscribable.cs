using UnityEngine;
using System.Collections.Generic;

public class MonoSingletonSubscribable<T, U> : MonoSingleton<U> where U : MonoBehaviour
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
