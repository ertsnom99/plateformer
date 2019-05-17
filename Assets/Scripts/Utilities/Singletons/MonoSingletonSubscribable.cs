using UnityEngine;
using System.Collections.Generic;

public class MonoSingletonSubscribable<T, U> : MonoSingleton<U> where U : MonoBehaviour
{
    protected List<T> Subscribers = new List<T>();

    public virtual void Subscribe(T subscriber)
    {
        Subscribers.Add(subscriber);
    }

    public virtual void Unsubscribe(T subscriber)
    {
        Subscribers.Remove(subscriber);
    }
}
