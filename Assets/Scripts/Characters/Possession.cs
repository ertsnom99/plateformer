using Cinemachine;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public class Possession : MonoBehaviour, IPhysicsObjectCollisionListener
{
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

    private Animator _animator;
    private AudioSource _audioSource;

    private void Awake()
    {
        InPossessionMode = false;
        // Change if player collides with AIs
        //Physics2D.IgnoreLayerCollision(GameManager.PlayerLayerIndex, GameManager.AILayerIndex, !InPossessionMode);

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
            
            // Change if player collides with AIs
            //Physics2D.IgnoreLayerCollision(GameManager.PlayerLayerIndex, GameManager.AILayerIndex, !InPossessionMode);

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
    public void TakePossession(AIController possessedController)
    {
        possessedController.Possess(this);
        ChangePossessionMode(false);

        gameObject.SetActive(false);
    }

    // Release possession of the AIController it is already in possession of, if any
    public void ReleasePossession()
    {
        VirtualCameraManager.Instance.ChangeVirtualCamera(_virtualCamera);

        gameObject.SetActive(true);
    }

    // Methods of the IPhysicsObjectCollisionListener interface
    public void OnPhysicsObjectCollisionEnter(PhysicsCollision2D collision)
    {
        if (InPossessionMode && collision.GameObject.tag == GameManager.EnemyTag)
        {
            TakePossession(collision.GameObject.GetComponent<AIController>());
        }
    }

    public void OnPhysicsObjectCollisionExit(PhysicsCollision2D collision) { }
}
