using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndSequence : MonoBehaviour, IFadeImageSubscriber
{
    [Header("Player Movement")]
    [SerializeField]
    private Inputs _forcedControls;

    [Header("Fade out")]
    [SerializeField]
    private FadeImage _fade;
    [SerializeField]
    private float _fadeDuration;

    [Header("Scene Change")]
    [SerializeField]
    private int _sceneToLoad;

    private void Start()
    {
        _fade.Subscribe(this);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(GameManager.PlayerTag))
        {
            PlayEndSequence(col.gameObject);
        }
        else if (col.CompareTag(GameManager.EnemyTag))
        {
            PossessableCharacterController controller = col.GetComponent<PossessableCharacterController>();

            if (controller.IsPossessed)
            {
                GameObject player = controller.Unpossess();

                PlayEndSequence(player);
            }
        }
    }

    private void PlayEndSequence(GameObject player)
    {
        player.GetComponent<PlayerController>().EnableControl(false);
        player.GetComponent<PlatformerMovement>().SetInputs(_forcedControls);

        _fade.FadeOut(_fadeDuration);
    }

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished() { }

    public void NotifyFadeOutFinished()
    {
        SceneManager.LoadScene(_sceneToLoad, LoadSceneMode.Single);
    }
}
