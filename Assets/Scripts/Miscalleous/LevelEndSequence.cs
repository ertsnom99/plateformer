using UnityEngine;

public class LevelEndSequence : MonoBehaviour
{
    [Header("Scene Change")]
    [SerializeField]
    private Inputs _forcedControls;
    [SerializeField]
    private int _sceneToLoad;
    [SerializeField]
    private bool _deleteAmbientManager = false;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            GameManager.Instance.LoadNextLevel(_forcedControls, _sceneToLoad, _deleteAmbientManager);
        }
        else if (col.CompareTag(GameManager.EnemyTag))
        {
            PossessablePawn enemy = col.GetComponent<PossessablePawn>();

            if (enemy.IsPossessed)
            {
                Inputs inputs = new Inputs();
                inputs.PressPossess = true;

                enemy.UpdateWithInputs(inputs);

                GameManager.Instance.LoadNextLevel(_forcedControls, _sceneToLoad);
            }
        }
    }
}
