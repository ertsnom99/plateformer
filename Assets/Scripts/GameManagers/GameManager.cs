using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
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

    [Header("Start Game")]
    [SerializeField]
    private Inputs _forcedControlsAtGameStart;

    private AmbientManager _ambientManagerToDelete;

    [Header("Navigation")]
    [SerializeField]
    private InputSystemUIInputModule _inputModule;

    [Header("Pause")]
    [SerializeField]
    private PauseMenu _pauseMenu;

    [Header("Level Change")]
    [SerializeField]
    private float _nextLevelFadeDuration = 1.0f;

    private int _sceneToLoad = -1;
    private bool _deleteAmbientManager = false;

    [Header("Quit Game")]
    [SerializeField]
    private Inputs _forcedControlsAtQuit;
    [SerializeField]
    private float _quitFadeDuration = 1.0f;

    public bool QuittingGame { get; private set; }

    public bool InLevelStartSequence { get; private set; }
    public bool InLevelEndSequence { get; private set; }

    private bool _forcingControls = false;
    private Inputs _currentForcedControls;

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

        // Disable mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!Fade)
        {
            Debug.LogError("No fade was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!PlayerController)
        {
            Debug.LogError("No player character was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_playerMovement)
        {
            Debug.LogError("No player movement was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_inputModule)
        {
            Debug.LogError("No input module was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        InLevelStartSequence = false;
        InLevelEndSequence = false;
    }

    // Methods used to react to inputs
    #region Input action callbacks
    /*public void OnPause(InputAction.CallbackContext input)
    {
        if (input.phase == InputActionPhase.Performed)
        {
            TogglePause();
        }
    }

    public void OnNavigate(InputAction.CallbackContext input)
    {
        if (_pauseMenu && _pauseMenu.gameObject.activeSelf)
        {
            // Select the first selected gameobject when none is while trying to navigate
            if (!EventSystem.current.currentSelectedGameObject && Mathf.Abs(input.ReadValue<Vector2>().y) > 0.9f)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
            }
        }
    }*/

    // HACK: Fixes bug with the Input System UI Input Module
    public void OnSubmit(InputAction.CallbackContext input)
    {
        if (_pauseMenu && _pauseMenu.gameObject.activeSelf)
        {
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
    }

    public void OnLoadLevel(InputAction.CallbackContext input)
    {
        LoadLevel(int.Parse(input.control.displayName));
    }
    #endregion

    protected virtual void Start()
    {
        Fade.Subscribe(this);
        Fade.SetOpacity(true);

        InLevelStartSequence = true;

        if (PlayerController)
        {
            PlayerController.EnableControl(false);
        }

        _playerMovement.ChangeOrientation(_startOrientedLeft ? Vector2.left : Vector2.right);
        ForceControls(_forcedControlsAtLevelStart);

        if (_pauseMenu)
        {
            ShowPause(false);
        }
        
        _ambientManagerToDelete = FindObjectOfType<AmbientManager>();

        Fade.FadeIn(FadeDuration);
    }

    private void Update()
    {
        if(_forcingControls && PlayerController?.GetControlledPawn())
        {
            PlayerController.GetControlledPawn().UpdateWithInputs(_currentForcedControls);
        }
    }

    public void ForceControls(Inputs controls)
    {
        _currentForcedControls = controls;
        _forcingControls = true;
    }

    public void StopForceControls()
    {
        _forcingControls = false;
        PlayerController.GetControlledPawn().UpdateWithInputs(new Inputs());
    }

    public void StartGame()
    {
        ForceControls(_forcedControlsAtGameStart);
        _inputModule.enabled = false;
    }
    
    public void TogglePause()
    {
        if (_pauseMenu && !Fade.IsFading)
        {
            ShowPause(!_pauseMenu.gameObject.activeSelf);
        }
    }

    public void ShowPause(bool show)
    {
        _pauseMenu.Show(show);
        Time.timeScale = show ? 0.0f : 1.0f;

        if (show)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
        }
    }

    public void ReloadCurrentLevel()
    {
        int level = SceneManager.GetActiveScene().buildIndex;

        if (level > -1)
        {
            LoadLevel(level);
        }
    }

    public void LoadLevel(int sceneToLoad)
    {
        if (PlayerController)
        {
            PlayerController.EnableControl(false);
        }

        _playerMovement.GetComponent<PlatformerMovement>().SetInputs(new Inputs());

        if (_pauseMenu)
        {
            ShowPause(false);
        }

        _sceneToLoad = sceneToLoad;
        InLevelEndSequence = true;


        Fade.FadeOut(_nextLevelFadeDuration);
    }

    public void LoadLevelAndDeleteAmbientManager(int sceneToLoad)
    {
        if (_ambientManagerToDelete)
        {
            _ambientManagerToDelete.FadeVolumeOut(_nextLevelFadeDuration);

            _deleteAmbientManager = true;
        }

        LoadLevel(sceneToLoad);
    }

    public void LoadNextLevel(Inputs forcedControls, int sceneToLoad, bool deleteAmbientManager = false)
    {
        if (PlayerController)
        {
            PlayerController.EnableControl(false);
        }

        ForceControls(forcedControls);
        
        if (deleteAmbientManager && _ambientManagerToDelete)
        {
            _ambientManagerToDelete.FadeVolumeOut(_nextLevelFadeDuration);
        }
        
        _sceneToLoad = sceneToLoad;
        InLevelEndSequence = true;

        _deleteAmbientManager = deleteAmbientManager;

        Fade.FadeOut(_nextLevelFadeDuration);
    }

    public void QuitGame()
    {
        ForceControls(_forcedControlsAtQuit);
        _inputModule.enabled = false;
    }

    public void QuitApplication()
    {
        if (PlayerController)
        {
            PlayerController.EnableControl(false);
        }

        _playerMovement.GetComponent<PlatformerMovement>().SetInputs(new Inputs());

        if (_pauseMenu)
        {
            ShowPause(false);
        }
        
        QuittingGame = true;

        Fade.FadeOut(_quitFadeDuration);
    }

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished()
    {
        if (InLevelStartSequence && _enableControlAfterFadeIn)
        {
            InLevelStartSequence = false;
            StopForceControls();

            if (PlayerController)
            {
                PlayerController.EnableControl(true);
            }
        }
    }

    public virtual void NotifyFadeOutFinished()
    {
        if (InLevelEndSequence)
        {
            InLevelEndSequence = false;
            SceneManager.LoadScene(_sceneToLoad, LoadSceneMode.Single);

            if (_deleteAmbientManager && _ambientManagerToDelete)
            {
                Destroy(_ambientManagerToDelete.gameObject);
            }
        }
        else if (QuittingGame)
        {
            QuittingGame = false;
            Application.Quit();
        }
    }
}
