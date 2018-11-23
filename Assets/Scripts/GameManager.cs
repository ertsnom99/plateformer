using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Tags
    public const string PlayerTag = "Player";

    private void Update ()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
