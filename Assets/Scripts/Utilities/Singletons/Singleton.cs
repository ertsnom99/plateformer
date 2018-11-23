using System;

/// <summary>
/// Singleton
/// </summary>
/// <remarks> 
/// Any class extending this class becomes a Singleton.
/// T is the class that extends this Singleton.
/// This is a lazy Singleton, therefore it will only be created the first time someone tries to access it.
/// </remarks>
public abstract class Singleton<T> where T : class
{
    private static T _instance;

    public static T Instance()
    {
        if (_instance == null)
        {
            _instance = CreateInstanceOfT();
        }

        return _instance;
    }

    private static T CreateInstanceOfT()
    {
        return Activator.CreateInstance(typeof(T), true) as T;
    }
}