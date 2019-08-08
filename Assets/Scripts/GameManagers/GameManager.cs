using UnityEngine;

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
    private bool _enableControlAfterFadeIn = true;
    [SerializeField]
    private Inputs _forcedControlsAtLevelStart;

    // Tags
    public const string PlayerTag = "Player";
    public const string EnemyTag = "Enemy";

    // Layers
    public const string PlayerLayer = "Player";
    public const string AILayer = "AI";

    protected virtual void Start()
    {
        Fade.Subscribe(this);
        Fade.SetOpacity(true);
        
        PlayerController.EnableControl(false);

        Fade.FadeIn(FadeDuration);
    }

    protected virtual void Update()
    {
        if (Input.GetButtonDown("Quit"))
        {
            Application.Quit();
        }
    }

    // Methods of the IFadeImageSubscriber interface
    public void NotifyFadeInFinished()
    {
        if (_enableControlAfterFadeIn)
        {
            PlayerController.EnableControl(true);
            _playerMovement.SetInputs(_forcedControlsAtLevelStart);
        }
    }

    public virtual void NotifyFadeOutFinished() { }
}
