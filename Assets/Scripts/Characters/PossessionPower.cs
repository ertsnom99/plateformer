using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

// This script requires thoses components and will be added if they aren't already there
/*[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(SpriteRenderer))]*/
[RequireComponent(typeof(BoxCollider2D))]
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

    public bool InPossessionMode { get; private set; }

    private ReadOnlyArray<InputDevice> _inputDevices;

    private const string _possessModeAnimationLayerName = "Possess Mode";
    private int _possessModeAnimationLayerIndex;

    /*// Layers Index
    private int _playerLayerIndex;
    private int _AILayerIndex;*/

    /*private PlayerController _playerController;
    private PlatformerMovement _movementScript;
    private SpriteRenderer _spriteRenderer;*/
    private Collider2D _collider;
    private Animator _animator;
    private AudioSource _audioSource;

    private void Awake()
    {
        InPossessionMode = false;
        /*// Change if player collides with AIs
        _playerLayerIndex = LayerMask.NameToLayer(GameManager.PlayerLayer);
        _AILayerIndex = LayerMask.NameToLayer(GameManager.AILayer);

        Physics2D.IgnoreLayerCollision(_playerLayerIndex, _AILayerIndex, !InPossessionMode);*/

        /*_playerController = GetComponent<PlayerController>();
        _movementScript = GetComponent<PlatformerMovement>();
        _spriteRenderer = GetComponent<SpriteRenderer>();*/
        _collider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        _possessModeAnimationLayerIndex = _animator.GetLayerIndex(_possessModeAnimationLayerName);
        _animator.SetLayerWeight(_possessModeAnimationLayerIndex, .0f);
    }

    public void SetInputDevices(ReadOnlyArray<InputDevice> inputDevices)
    {
        _inputDevices = inputDevices;
    }

    public void ChangePossessionMode(bool inPossessionMode)
    {
        if (_inputDevices.Count <= 0)
        {
            Debug.LogError("No InputDevice was given to the " + GetType() + "script of " + gameObject.name + "!");
            return;
        }

        if (inPossessionMode != InPossessionMode)
        {
            InPossessionMode = inPossessionMode;
            _animator.SetLayerWeight(_possessModeAnimationLayerIndex, InPossessionMode ? 1.0f : .0f);

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

    // Take possession of the given AIController
    public void TakePossession(IPossessable possessed)
    {
        if (possessed.Possess(this, _inputDevices))
        {
            ChangePossessionMode(false);
            gameObject.SetActive(false);
            
            /*_playerController.EnableControl(false);
            _spriteRenderer.enabled = false;
            _collider.enabled = false;
            _movementScript.enabled = false;*/
        }
    }

    // Release possession of the AIController that it is in possession of, if it's the case
    public void ReleasePossession(Vector2 respawnPos, Vector2 respawnFacingDirection, bool centerPosColliderToRespawnPos = false)
    {
        // Replace the character
        if (centerPosColliderToRespawnPos)
        {
            gameObject.transform.position = respawnPos - _collider.offset;
        }
        else
        {
            gameObject.transform.position = respawnPos;
        }

        //_movementScript.ChangeOrientation(respawnFacingDirection);

        // Change the camera
        VirtualCameraManager.Instance.ChangeVirtualCamera(_virtualCamera);

        // Show the character
        gameObject.SetActive(true);
        /*_playerController.EnableControl(true);
        _spriteRenderer.enabled = true;
        _collider.enabled = true;
        _movementScript.enabled = true;*/
    }

    // Methods of the IPhysicsObjectCollisionListener interface
    public void OnPhysicsCollision2DEnter(PhysicsCollision2D collision) { }

    public void OnPhysicsCollision2DStay(PhysicsCollision2D collision)
    {
        if (InPossessionMode)
        {
            IPossessable possessableScript = collision.GameObject.GetComponent<IPossessable>();

            if (possessableScript != null)
            {
                TakePossession(possessableScript);
            }
        }
    }

    public void OnPhysicsCollision2DExit(PhysicsCollision2D collision) { }
}
