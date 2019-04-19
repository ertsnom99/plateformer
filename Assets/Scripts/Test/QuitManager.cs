using UnityEngine;

public class QuitManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetButtonDown("Quit"))
        {
            Application.Quit();
        }
    }
}
