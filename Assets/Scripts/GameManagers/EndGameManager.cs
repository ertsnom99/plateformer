using UnityEngine;

public class EndGameManager : GameManager
{
    protected override void Update()
    {
        base.Update();

        if (!InLevelEndSequence)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadNextLevel(new Inputs(), Level1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadNextLevel(new Inputs(), Level2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                LoadNextLevel(new Inputs(), Level3);
            }
        }
    }
}
