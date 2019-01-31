using UnityEngine;

public class GameManager : MonoBehaviour, IFadeImageSubscriber
{
    [Header("Fade out")]
    [SerializeField]
    private FadeImage m_fade;
    [SerializeField]
    private float m_fadeDuration;

    [Header("Player")]
    [SerializeField]
    private PlayerControl m_playerControl;
    [SerializeField]
    private PlatformerMovement m_playerMovement;
    [SerializeField]
    private bool m_enableControlAfterFadeIn = true;

    [Header("Player Movement")]
    [SerializeField]
    private Inputs m_forcedControls;

    // Tags
    public const string PlayerTag = "Player";

    private void Start()
    {
        m_fade.Subscribe(this);
        m_fade.SetOpacity(true);
        m_fade.FadeIn(m_fadeDuration);

        m_playerControl.EnableControl(false);
        m_playerMovement.SetInputs(m_forcedControls);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished()
    {
        if(m_enableControlAfterFadeIn)
        {
            m_playerControl.EnableControl(true);
        }
    }

    public void NotifyFadeOutFinished() { }
}
