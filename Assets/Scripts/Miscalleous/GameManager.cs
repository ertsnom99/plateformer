using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>, IHealthSubscriber, IFadeImageSubscriber
{
    [Header("Fade out")]
    [SerializeField]
    private FadeImage m_fade;
    [SerializeField]
    private float m_fadeDuration;

    private bool m_fading = false;

    [Header("Player")]
    [SerializeField]
    private PlayerControl m_playerControl;
    [SerializeField]
    private PlatformerMovement m_playerMovement;
    [SerializeField]
    private bool m_enableControlAfterFadeIn = true;
    [SerializeField]
    private Inputs m_forcedControls;
    
    [Header("Enemies")]
    [SerializeField]
    private EnemieRespawner m_enemieRespawner;

    [Header("End Game")]
    [SerializeField]
    private GameObject m_endGameWonText;
    [SerializeField]
    private GameObject m_endGameLoseText;
    [SerializeField]
    private int m_sceneToLoadOnWon = 0;
    [SerializeField]
    private int m_sceneToLoadOnLose = 1;

    private bool m_gameEnded = false;
    private bool m_gameWon = false;

    // Tags
    public const string PlayerTag = "Player";
    public const string EnemieTag = "Enemie";

    private void Start()
    {
        m_fade.Subscribe(this);
        m_fade.SetOpacity(true);
        m_fade.FadeIn(m_fadeDuration);

        m_playerControl.GetComponent<Health>().Subscribe(this);
        m_playerControl.EnableControl(false);
        m_playerMovement.SetInputs(m_forcedControls);
    }

    private void Update()
    {
        /*if (Input.GetButtonDown("Fire2"))
        {
            EndGame(false);
        }*/

        if (Input.GetButtonDown("Quit"))
        {
            Application.Quit();
        }
        else if (m_gameEnded && !m_fading && Input.GetButtonDown("Restart"))
        {
            m_fade.FadeOut(m_fadeDuration);
            m_fading = true;
        }
    }

    public void EndGame(bool won)
    {
        if (!m_gameEnded)
        {
            // Disable all enemies
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(EnemieTag);

            foreach (GameObject enemie in enemies)
            {
                enemie.GetComponent<ProximityExplodable>().enabled = false;
                enemie.GetComponent<AIControl>().EnableControl(false);
            }

            if (m_enemieRespawner)
            {
                m_enemieRespawner.EnableEnemieSpawn(false);
            }

            // Disable player
            m_playerControl.EnableControl(false);

            // Show end text
            if (won)
            {
                m_endGameWonText.SetActive(true);
            }
            else
            {
                m_endGameLoseText.SetActive(true);
            }

            m_gameEnded = true;
            m_gameWon = won;
        }
    }

    // Methods of the IHealable interface
    public void NotifyDamageApplied(Health healthScript, int damage) { }

    public void NotifyHealthChange(Health healthScript, int health) { }

    public void NotifyHealthDepleted(Health healthScript)
    {
        EndGame(false);
    }

    public void NotifyJustSubscribed(Health healthScript) { }

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished()
    {
        if(m_enableControlAfterFadeIn)
        {
            m_playerControl.EnableControl(true);
        }
    }

    public void NotifyFadeOutFinished()
    {
        if (m_gameEnded)
        {
            // Destroy the ambiant since the game is restarting
            Destroy(AmbiantManager.Instance.gameObject);
        }

        if (m_gameWon)
        {
            SceneManager.LoadScene(m_sceneToLoadOnWon, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(m_sceneToLoadOnLose, LoadSceneMode.Single);
        }
    }
}
