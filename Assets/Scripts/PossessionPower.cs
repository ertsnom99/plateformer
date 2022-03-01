using Cinemachine;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public class PossessionPower : MonoBehaviour, IPhysicsCollision2DListener
{
    [Header("Camera")]
    [SerializeField]
    private CinemachineVirtualCamera _virtualCamera;

    [Header("Sounds")]
    [SerializeField]
    private AudioClip _enterPossessionModeSound;
    [SerializeField]
    private AudioClip _exitPossessionModeSound;

    private Pawn _character;

    public bool InPossessionMode { get; private set; }

    private const string _possessModeAnimationLayerName = "Possess Mode";
    private int _possessModeAnimationLayerIndex;

    public delegate void ShowCharacterDelegate(bool show);
    private ShowCharacterDelegate _showCharacter;

    public delegate void ChangeOrientationDelegate(Vector2 facingDirection);
    private ChangeOrientationDelegate _changeOrientation;

    private Collider2D _collider;
    private Animator _animator;
    private AudioSource _audioSource;

    private void Awake()
    {
        InPossessionMode = false;
        
        _collider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        _possessModeAnimationLayerIndex = _animator.GetLayerIndex(_possessModeAnimationLayerName);
        _animator.SetLayerWeight(_possessModeAnimationLayerIndex, .0f);
    }

    public void SetPossessingCharacter(Pawn character)
    {
        _character = character;
    }

    public void SetShowCharacterDelegate(ShowCharacterDelegate showCharacterDelegate)
    {
        _showCharacter = showCharacterDelegate;
    }

    public void SetChangeOrientationDelegate(ChangeOrientationDelegate changeOrientationCallback)
    {
        _changeOrientation = changeOrientationCallback;
    }

    public void ChangePossessionMode(bool inPossessionMode, bool skipSound = false)
    {
        if (inPossessionMode != InPossessionMode)
        {
            InPossessionMode = inPossessionMode;
            _animator.SetLayerWeight(_possessModeAnimationLayerIndex, InPossessionMode ? 1.0f : .0f);

            if (skipSound)
            {
                return;
            }

            if (InPossessionMode)
            {
                _audioSource.PlayOneShot(_enterPossessionModeSound);
            }
            else
            {
                _audioSource.PlayOneShot(_exitPossessionModeSound);
            }
        }
    }

    private void TakePossession(PossessablePawn possessablePawn)
    {
        Controller tempController = _character.Controller;
        tempController.SetControlledPawn(null);

        if (possessablePawn.Possess(tempController, _collider.bounds, Physics2D.GetLayerCollisionMask(gameObject.layer), ReleasePossession))
        {
            ChangePossessionMode(false, true);
            _showCharacter(false);
        }
        else
        {
            tempController.SetControlledPawn(_character);
        }
    }

    // Release possession of the AIController that it is in possession of, if it's the case
    public void ReleasePossession(Controller controller, Vector2 spawnPosition, Vector2 spawnFacingDirection)
    {
        // Replace the pawn
        gameObject.transform.position = spawnPosition - _collider.offset;

        // Change the camera
        VirtualCameraManager.Instance.ChangeVirtualCamera(_virtualCamera);

        controller.SetControlledPawn(_character);

        // Most be orientated after controller is set!
        if (_changeOrientation != null)
        {
            _changeOrientation(spawnFacingDirection);
        }

        _showCharacter(true);
    }

    // Methods of the IPhysicsObjectCollisionListener interface
    public void OnPhysicsCollision2DEnter(PhysicsCollision2D collision) { }

    public void OnPhysicsCollision2DStay(PhysicsCollision2D collision)
    {
        if (InPossessionMode)
        {
            PossessablePawn possessablePawn = collision.GameObject.GetComponent<PossessablePawn>();

            if (possessablePawn != null)
            {
                TakePossession(possessablePawn);
            }
        }
    }

    public void OnPhysicsCollision2DExit(PhysicsCollision2D collision) { }
}
