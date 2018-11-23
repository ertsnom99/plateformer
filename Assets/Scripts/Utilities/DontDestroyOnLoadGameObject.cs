using UnityEngine;

public class DontDestroyOnLoadGameObject : MonoBehaviour
{
	private void Awake ()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(gameObject.tag);

        if (objects.Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);
	}
}
