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

    // Tags
    public const string PlayerTag = "Player";

    private void Start()
    {
        m_fade.Subscribe(this);
        m_fade.SetOpacity(true);
        m_fade.FadeIn(m_fadeDuration);

        m_playerControl.EnableControl(false);
    }

    private void Update ()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished()
    {
        m_playerControl.EnableControl(true);
    }

    public void NotifyFadeOutFinished() { }
}
