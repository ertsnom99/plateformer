using System.Collections.Generic;

public abstract class SubscribablePossessableCharacterController<T> : PossessableCharacterController
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
