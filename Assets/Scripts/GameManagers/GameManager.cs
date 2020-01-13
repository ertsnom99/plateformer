﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

    [Header("Pause")]
    [SerializeField]
    private GameObject _pauseMenu;

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

        // Disable mouse
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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

    // Methods used to react to inputs
    #region Input action callbacks
    public void OnPause(InputAction.CallbackContext input)
    {
        if (_pauseMenu && input.phase == InputActionPhase.Performed && !Fade.IsFading)
        {
            ShowPause(!_pauseMenu.activeSelf);
        }
    }

    public void OnNavigate(InputAction.CallbackContext input)
    {
        if (_pauseMenu && _pauseMenu.activeSelf)
        {
            // Select the first selected gameobject when none is while trying to navigate
            if (!EventSystem.current.currentSelectedGameObject && Mathf.Abs(input.ReadValue<Vector2>().y) > 0.9f)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
            }
        }
    }

    // HACK: Fixes bug with the Input System UI Input Module
    public void OnSubmit(InputAction.CallbackContext input)
    {
        if (_pauseMenu && _pauseMenu.activeSelf)
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

        PlayerController.EnableControl(false);
        _playerMovement.ChangeOrientation(_startOrientedLeft ? Vector2.left : Vector2.right);
        _playerMovement.SetInputs(_forcedControlsAtLevelStart);

        if (_pauseMenu)
        {
            ShowPause(false);
        }

        Fade.FadeIn(FadeDuration);
    }

    public void ShowPause(bool show)
    {
        _pauseMenu.SetActive(show);
        Time.timeScale = show ? 0.0f : 1.0f;

        if (show && !EventSystem.current.currentSelectedGameObject)
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
        PlayerController.EnableControl(false);
        _playerMovement.GetComponent<PlatformerMovement>().SetInputs(new Inputs());

        if (_pauseMenu)
        {
            ShowPause(false);
        }

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
