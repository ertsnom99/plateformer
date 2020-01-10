using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>, IFadeImageSubscriber
{
    [Header("Fade out")]
    [SerializeField]
    protected FadeImage Fade;
    [SerializeField]
    protected float FadeDuration = 1.0f;
    
    [Header("Player")]
    [SerializeField]
    protected PlayerController PlayerController;
    [SerializeField]
    private PlatformerMovement _playerMovement;
    [SerializeField]
    private bool _startOrientedLeft = false;
    [SerializeField]
    private bool _enableControlAfterFadeIn = true;
    [SerializeField]
    private Inputs _forcedControlsAtLevelStart;

    [Header("Level Change")]
    [SerializeField]
    private float _nextLevelFadeDuration = 1.0f;

    private int _sceneToLoad = -1;

    public bool InLevelStartSequence { get; private set; }
    public bool InLevelEndSequence { get; private set; }

    // Levels
    public const int Level1 = 1;
    public const int Level2 = 2;
    public const int Level3 = 3;
    public const int Level4 = 4;

    // Tags
    public const string PlayerTag = "Player";
    public const string EnemyTag = "Enemy";
    public const string InteractableTag = "Interactable";

    // Layers
    public const string PlayerLayer = "Player";
    public const string AILayer = "AI";

    protected override void Awake()
    {
        base.Awake();

        if (!Fade)
        {
            Debug.LogError("No fade was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!PlayerController)
        {
            Debug.LogError("No player controller was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_playerMovement)
        {
            Debug.LogError("No player movement was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        InLevelStartSequence = false;
        InLevelEndSequence = false;
    }

    protected virtual void Start()
    {
        Fade.Subscribe(this);
        Fade.SetOpacity(true);

        InLevelStartSequence = true;

        PlayerController.EnableControl(false);
        _playerMovement.ChangeOrientation(_startOrientedLeft ? Vector2.left : Vector2.right);
        _playerMovement.SetInputs(_forcedControlsAtLevelStart);

        Fade.FadeIn(FadeDuration);
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }

        if (Input.GetKey(KeyCode.L) && Input.GetKey(KeyCode.Alpha1))
        {
            LoadLevel(Level1);
        }
        else if (Input.GetKey(KeyCode.L) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            LoadLevel(Level2);
        }
        else if (Input.GetKey(KeyCode.L) && Input.GetKeyDown(KeyCode.Alpha3))
        {
            LoadLevel(Level3);
        }
        else if (Input.GetKey(KeyCode.L) && Input.GetKeyDown(KeyCode.Alpha4))
        {
            LoadLevel(Level4);
        }
    }

    public void LoadLevel(int sceneToLoad)
    {
        PlayerController.EnableControl(false);
        _playerMovement.GetComponent<PlatformerMovement>().SetInputs(new Inputs());

        _sceneToLoad = sceneToLoad;
        InLevelEndSequence = true;

        Fade.FadeOut(_nextLevelFadeDuration);
    }

    public void LoadNextLevel(Inputs forcedControls, int sceneToLoad)
    {
        PlayerController.EnableControl(false);
        _playerMovement.GetComponent<PlatformerMovement>().SetInputs(forcedControls);
        
        _sceneToLoad = sceneToLoad;
        InLevelEndSequence = true;

        Fade.FadeOut(_nextLevelFadeDuration);
    }

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished()
    {
        if (InLevelStartSequence && _enableControlAfterFadeIn)
        {
            InLevelStartSequence = false;
            PlayerController.EnableControl(true);
        }
    }

    public virtual void NotifyFadeOutFinished()
    {
        if (InLevelEndSequence)
        {
            InLevelEndSequence = false;
            SceneManager.LoadScene(_sceneToLoad, LoadSceneMode.Single);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
