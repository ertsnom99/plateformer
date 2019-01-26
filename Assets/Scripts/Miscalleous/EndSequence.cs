using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSequence : MonoBehaviour, IFadeImageSubscriber
{
    [Header("Camera")]
    [SerializeField]
    private VirtualCameraManager m_virtualCameraManager;

    [Header("Player Movement")]
    [SerializeField]
    private Inputs m_forcedControls;

    [Header("Fade out")]
    [SerializeField]
    private FadeImage m_fade;
    [SerializeField]
    private float m_fadeDuration;

    [Header("Scene Change")]
    [SerializeField]
    private int m_sceneToLoad;

    private void Start()
    {
        m_fade.Subscribe(this);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (m_virtualCameraManager.ActiveVirtualCamera && col.CompareTag(GameManager.PlayerTag))
        {
            m_virtualCameraManager.ActiveVirtualCamera.Follow = null;

            col.GetComponent<PlayerControl>().EnableControl(false);
            col.GetComponent<PlatformerMovement>().SetInputs(m_forcedControls);

            m_fade.FadeOut(m_fadeDuration);
        }
    }

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished() { }

    public void NotifyFadeOutFinished()
    {
        // TODO: Load level
        Debug.Log("Load...");
        SceneManager.LoadScene(m_sceneToLoad, LoadSceneMode.Single);
    }
}
