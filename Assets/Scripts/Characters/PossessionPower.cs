using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public class PossessionPower : MonoBehaviour, IPhysicsCollision2DListener
{
    [Header("Collision")]
    // The collider that needs to be consider when respawning from possession (if it IS considered)
    [SerializeField]
    private Collider2D _collider;

    [Header("Camera")]
    [SerializeField]
    private CinemachineVirtualCamera _virtualCamera;

    [Header("Sounds")]
    [SerializeField]
    private AudioClip _enterPossessionModeSound;
    [SerializeField]
    private AudioClip _exitPossessionModeSound;

    public bool InPossessionMode { get; private set; }

    private const string _possessModeAnimationLayerName = "Possess Mode";
    private int _possessModeAnimationLayerIndex;
    
    /*// Layers Index
    private int _playerLayerIndex;
    private int _AILayerIndex;*/

    private PlatformerMovement _movementScript;
    private Animator _animator;
    private AudioSource _audioSource;

    private void Awake()
    {
        InPossessionMode = false;
        /*// Change if player collides with AIs
        _playerLayerIndex = LayerMask.NameToLayer(GameManager.PlayerLayer);
        _AILayerIndex = LayerMask.NameToLayer(GameManager.AILayer);

        Physics2D.IgnoreLayerCollision(_playerLayerIndex, _AILayerIndex, !InPossessionMode);*/

        if (!_collider)
        {
            Debug.LogError("No collider was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        _movementScript = GetComponent<PlatformerMovement>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        _possessModeAnimationLayerIndex = _animator.GetLayerIndex(_possessModeAnimationLayerName);
        _animator.SetLayerWeight(_possessModeAnimationLayerIndex, .0f);
    }

    public void ChangePossessionMode(bool inPossessionMode)
    {
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
        if (possessed.Possess(this))
        {
            ChangePossessionMode(false);
            gameObject.SetActive(false);
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

        _movementScript.ChangeFacingDirection(respawnFacingDirection);

        // Change the camera
        VirtualCameraManager.Instance.ChangeVirtualCamera(_virtualCamera);

        // Show the character
        gameObject.SetActive(true);
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
