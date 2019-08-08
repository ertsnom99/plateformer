using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndSequence : MonoBehaviour, IFadeImageSubscriber
{
    [Header("Camera")]
    [SerializeField]
    private VirtualCameraManager _virtualCameraManager;

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
        if (_virtualCameraManager.ActiveVirtualCamera && col.CompareTag(GameManager.PlayerTag))
        {
            _virtualCameraManager.ActiveVirtualCamera.Follow = null;

            col.GetComponent<PlayerController>().EnableControl(false);
            col.GetComponent<PlatformerMovement>().SetInputs(_forcedControls);

            _fade.FadeOut(_fadeDuration);
        }
    }

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished() { }

    public void NotifyFadeOutFinished()
    {
        SceneManager.LoadScene(_sceneToLoad, LoadSceneMode.Single);
    }
}
