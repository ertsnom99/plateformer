using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>, IHealthSubscriber, IFadeImageSubscriber
{
    [Header("Fade out")]
    [SerializeField]
    private FadeImage _fade;
    [SerializeField]
    private float _fadeDuration;

    private bool _fading = false;

    [Header("Player")]
    [SerializeField]
    private PlayerControl _playerControl;
    [SerializeField]
    private PlatformerMovement _playerMovement;
    [SerializeField]
    private bool _enableControlAfterFadeIn = true;
    [SerializeField]
    private Inputs _forcedControlsAtLevelStart;
    
    [Header("Enemies")]
    [SerializeField]
    private EnemieRespawner _enemieRespawner;

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

    // Tags
    public const string PlayerTag = "Player";
    public const string EnemieTag = "Enemie";

    private void Start()
    {
        _fade.Subscribe(this);
        _fade.SetOpacity(true);
        _fade.FadeIn(_fadeDuration);

        _playerControl.GetComponent<Health>().Subscribe(this);
        _playerControl.EnableControl(false);
        _playerMovement.SetInputs(_forcedControlsAtLevelStart);
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
        else if (_gameEnded && !_fading && Input.GetButtonDown("Restart"))
        {
            _fade.FadeOut(_fadeDuration);
            _fading = true;
        }
    }

    public void EndGame(bool won)
    {
        if (!_gameEnded)
        {
            // Disable all enemies
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(EnemieTag);

            foreach (GameObject enemie in enemies)
            {
                enemie.GetComponent<ProximityExplodable>().enabled = false;
                enemie.GetComponent<AIControl>().EnableControl(false);
            }

            if (_enemieRespawner)
            {
                _enemieRespawner.EnableEnemieSpawn(false);
            }

            // Disable player
            _playerControl.EnableControl(false);

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
        if(_enableControlAfterFadeIn)
        {
            _playerControl.EnableControl(true);
        }
    }

    public void NotifyFadeOutFinished()
    {
        if (_gameEnded)
        {
            // Destroy the ambiant since the game is restarting
            Destroy(AmbiantManager.Instance.gameObject);
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
