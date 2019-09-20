using UnityEngine;

public class EndGameManager : GameManager
{
    [SerializeField]
    private int _level1 = 0;
    [SerializeField]
    private int _level2 = 0;
    [SerializeField]
    private int _level3 = 0;

    protected override void Update()
    {
        base.Update();

        if (!InLevelEndSequence)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadNextLevel(new Inputs(), _level1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadNextLevel(new Inputs(), _level2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                LoadNextLevel(new Inputs(), _level3);
            }
        }
    }
}
