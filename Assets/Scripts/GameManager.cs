using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Update ()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
