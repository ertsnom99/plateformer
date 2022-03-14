using Cinemachine;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]

public abstract class PossessablePawn : Pawn
{
    protected Inputs NoControlInputs = new Inputs();

    [Header("Possession")]
    [SerializeField]
    private bool _isPossessable = true;

    public bool IsPossessable
    {
        get { return _isPossessable; }
        private set { _isPossessable = value; }
    }

    public bool IsPossessed { get; protected set; }

    private Controller _previousController = null;

    [SerializeField]
    protected CinemachineVirtualCamera PossessionVirtualCamera;

    protected Bounds UnpossessBounds;
    protected ContactFilter2D ContactFilter = new ContactFilter2D();

    [SerializeField]
    private Vector2 _unpossessShell = new Vector2(.1f, .1f);

    public delegate void UnpossessCallbackDelegate(Controller controller, Vector2 spawnPosition, Vector2 respawnFacingDirection);
    protected UnpossessCallbackDelegate OnUnpossessCallback;

    [SerializeField]
    protected AudioClip OnPossessSound;
    [SerializeField]
    protected AudioClip OnUnpossessSound;

    protected const string PossessedModeAnimationLayerName = "Possessed Mode";
    private int PossessedModeAnimationLayerIndex;

    protected SpriteRenderer SpriteRenderer;
    protected Collider2D Collider;
    private Animator _animator;
    protected AudioSource AudioSource;

    protected virtual void Awake()
    {
        IsPossessed = false;

        ContactFilter.useLayerMask = true;
        ContactFilter.useTriggers = false;

        SpriteRenderer = GetComponent<SpriteRenderer>();
        Collider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();

        PossessedModeAnimationLayerIndex = _animator.GetLayerIndex(PossessedModeAnimationLayerName);

        UpdateVisual();
    }
#if UNITY_EDITOR
    protected virtual void Start()
    {
        if (!VirtualCameraManager.Instance)
        {
            Debug.LogError("Couldn't find an instance of VirtualCameraManager for " + GetType() + " script of " + gameObject.name + "!");
        }
        
        if (!PossessionVirtualCamera)
        {
            Debug.LogError("No virtual camera was set for " + GetType() + " script of " + gameObject.name + "!");
        }
    }
#endif
    public void SetPossessionVirtualCamera(CinemachineVirtualCamera possessionVirtualCamera)
    {
        PossessionVirtualCamera = possessionVirtualCamera;
    }

    public override void UpdateWithInputs(Inputs inputs)
    {
        UpdatePossession(inputs);
    }

    protected virtual void UpdatePossession(Inputs inputs)
    {
        Vector2 spawnPosition = Vector2.zero;
        Vector2 spawnFacingDirection = Vector2.zero;

        if (inputs.PressPossess && HasEnoughSpaceToUnpossess(ref spawnPosition, ref spawnFacingDirection))
        {
            Unpossess(spawnPosition, spawnFacingDirection);
        }
    }

    // Returns if the area to respawn is free. The area checked is based on the facing direction of the pawn
    protected virtual bool HasEnoughSpaceToUnpossess(ref Vector2 spawnPosition, ref Vector2 spawnFacingDirection)
    {
        spawnPosition.y = CalculateSpawnY();

        if (LookingLeft())
        {
            spawnPosition.x = CalculateLeftSpawnX();
            spawnFacingDirection = Vector2.left;
        }
        else
        {
            spawnPosition.x = CalculateRightSpawnX();
            spawnFacingDirection = Vector2.right;
        }
#if UNITY_EDITOR
        DrawRect(new Rect(spawnPosition.x - UnpossessBounds.size.x / 2.0f, spawnPosition.y - UnpossessBounds.size.y / 2.0f, UnpossessBounds.size.x, UnpossessBounds.size.y), 5.0f);
#endif
        return Physics2D.OverlapBox(spawnPosition, UnpossessBounds.size, .0f, ContactFilter, new Collider2D[1]) <= 0;
    }

    protected float CalculateSpawnY()
    {
        return Collider.bounds.center.y - Collider.bounds.extents.y + UnpossessBounds.extents.y + _unpossessShell.y;
    }

    protected float CalculateLeftSpawnX()
    {
        return Collider.bounds.center.x - (Collider.bounds.extents.x + UnpossessBounds.extents.x + _unpossessShell.x);
    }

    protected float CalculateRightSpawnX()
    {
        return Collider.bounds.center.x + Collider.bounds.extents.x + UnpossessBounds.extents.x + _unpossessShell.x;
    }

    protected virtual bool LookingLeft()
    {
        return SpriteRenderer.flipX;
    }
#if UNITY_EDITOR
    protected void DrawRect(Rect rect, float duration)
    {
        Debug.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x + rect.width, rect.y), Color.green, duration);
        Debug.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x, rect.y + rect.height), Color.red, duration);
        Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y + rect.height), new Vector3(rect.x + rect.width, rect.y), Color.green, duration);
        Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y + rect.height), new Vector3(rect.x, rect.y + rect.height), Color.red, duration);
    }
#endif
    public void EnablePossession(bool enable)
    {
        IsPossessable = enable;
    }

    public bool Possess(Controller controller, Bounds unpossessBounds, LayerMask unpossessCollisionMask, UnpossessCallbackDelegate onUnpossessCallback, bool skipSound = false)
    {
        if (IsPossessable && !IsPossessed)
        {
            IsPossessed = true;

            // Cache current controller
            if (Controller)
            {
                _previousController = Controller;
                Controller.SetControlledPawn(null);
            }

            UnpossessBounds = unpossessBounds;
            ContactFilter.layerMask = unpossessCollisionMask;

            controller.SetControlledPawn(this);
            this.OnUnpossessCallback = onUnpossessCallback;
            
            VirtualCameraManager.Instance.ChangeVirtualCamera(PossessionVirtualCamera);

            if (!skipSound)
            {
                AudioSource.pitch = Random.Range(.9f, 1.0f);
                AudioSource.PlayOneShot(OnPossessSound);
            }

            UpdateVisual();
            OnPossess();
        }

        return IsPossessed;
    }

    protected virtual void OnPossess() { }

    public void Unpossess(Vector2 spawnPosition, Vector2 spawnFacingDirection, bool skipSound = false)
    {
        if (IsPossessable && IsPossessed)
        {
            IsPossessed = false;

            Controller tempController = Controller;
            tempController.SetControlledPawn(null);

            OnUnpossessCallback(tempController, spawnPosition, spawnFacingDirection);
            OnUnpossessCallback = null;

            // Restore original controller
            if (_previousController)
            {
                _previousController.SetControlledPawn(this);
                _previousController = null;
            }

            if (!skipSound)
            {
                AudioSource.pitch = Random.Range(.9f, 1.0f);
                AudioSource.PlayOneShot(OnUnpossessSound);
            }

            UpdateVisual();
            OnUnpossess();
        }
    }

    protected virtual void OnUnpossess() { }

    protected void UpdateVisual()
    {
        _animator.SetLayerWeight(PossessedModeAnimationLayerIndex, IsPossessed ? 1.0f : .0f);
    }
}
