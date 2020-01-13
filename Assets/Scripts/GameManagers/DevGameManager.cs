using UnityEngine;
using UnityEngine.SceneManagement;

public class DevGameManager : GameManager, IHealthSubscriber
{
    private bool _fading = false;

    [Header("Enemies")]
    [SerializeField]
    private EnemySpawnManager _enemySpawnManager;

    [Header("End Game")]
    [SerializeField]
    private GameObject _endGameWonText;
    [SerializeField]
    private GameObject _endGameLoseText;
    [SerializeField]
    private int _sceneToLoadOnWon = 0;
    [SerializeField]
    private int _sceneToLoadOnLose = 1;

    private bool _gameEnded = false;
    private bool _gameWon = false;

    protected override void Awake()
    {
        base.Awake();

        if (!_enemySpawnManager)
        {
            Debug.LogError("No enemy spawn manager was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_endGameWonText)
        {
            Debug.LogError("No end game won text was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_endGameLoseText)
        {
            Debug.LogError("No end game lose text was set for " + GetType() + " script of " + gameObject.name + "!");
        }
    }

    protected override void Start()
    {
        base.Start();

        PlayerController.GetComponent<Health>().Subscribe(this);
    }

    protected void Update()
    {
        if (_gameEnded && !_fading && Input.GetButtonDown("Restart"))
        {
            Fade.FadeOut(FadeDuration);
            _fading = true;
        }
    }

    public void EndGame(bool won)
    {
        if (!_gameEnded)
        {
            // Disable all enemies
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(EnemyTag);

            foreach (GameObject enemie in enemies)
            {
                enemie.GetComponent<Explodable>().enabled = false;
                enemie.GetComponent<PossessableCharacterController>().EnableControl(false);
            }

            if (_enemySpawnManager)
            {
                _enemySpawnManager.EnableEnemieSpawn(false);
            }

            // Disable player
            PlayerController.EnableControl(false);

            // Show end text
            if (won)
            {
                _endGameWonText.SetActive(true);
            }
            else
            {
                _endGameLoseText.SetActive(true);
            }

            _gameEnded = true;
            _gameWon = won;
        }
    }

    // Methods of the IHealable interface
    public void NotifyDamageCalled(Health healthScript, int damage) { }

    public void NotifyHealCalled(Health healthScript, int gain) { }

    public void NotifyHealthChange(Health healthScript, int health) { }

    public void NotifyHealthDepleted(Health healthScript)
    {
        EndGame(false);
    }

    public void NotifyJustSubscribed(Health healthScript) { }

    // Methods of the IFadeImageSubscriber interface
    public override void NotifyFadeOutFinished()
    {
        if (_gameEnded)
        {
            // Destroy the ambiant since the game is restarting
            Destroy(AmbientManager.Instance.gameObject);
        }

        if (_gameWon)
        {
            SceneManager.LoadScene(_sceneToLoadOnWon, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(_sceneToLoadOnLose, LoadSceneMode.Single);
        }
    }
}
