using UnityEngine;

/// <summary>
/// KeptMonoSingleton
/// </summary>
/// <remarks> 
/// Any class extending this class becomes a Singleton and a MonoBehaviour.
/// This Singleton won't be destroyed on load.
/// T is the class that extends this singleton.
/// This is a lazy singleton, therefore it will only be created the first time someone tries to access it.
/// </remarks>
public class KeptMonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();

                        DontDestroyOnLoad(singleton);

                        Debug.Log("[Singleton] An instance of " + typeof(T) + " is needed in the scene, so '" + singleton + "' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        DontDestroyOnLoad(_instance.gameObject);
                        Debug.Log("[Singleton] Using instance already created: " + _instance.gameObject.name);
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it has been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after the Application stopped playing.
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    public void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = GetComponent<T>();
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != GetComponent<T>())
        {
            Destroy(gameObject);
        }
    }
}