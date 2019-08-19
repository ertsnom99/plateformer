using Pathfinding;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(BulletMovement))]
[RequireComponent(typeof(ExplodableBullet))]

public class BulletController : PossessableCharacterController, IPhysicsCollision2DListener, IProximityExplodableSubscriber
{
    [Header("Controls")]
    [SerializeField]
    float _minMagnitudeToAim = .8f;

    BulletMovement _movementScript;
    ExplodableBullet _explodableBullet;

    protected override void Awake()
    {
        // Tells to use the layer settings from the Physics2D settings (the matrix)
        LeftPlayerSpawnContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(LeftPlayerSpawn.gameObject.layer));
        LeftPlayerSpawnContactFilter.useLayerMask = true;
        LeftPlayerSpawnContactFilter.useTriggers = false;

        IsPossessed = false;
        
        Animator = GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();
        _movementScript = GetComponent<BulletMovement>();
        Seeker = GetComponent<Seeker>();
        _explodableBullet = GetComponent<ExplodableBullet>();

        PossessedModeAnimationLayerIndex = Animator.GetLayerIndex(PossessedModeAnimationLayerName);
        Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, .0f);

        if (!LeftPlayerSpawn)
        {
            Debug.LogError("No left player spawn was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        _explodableBullet.Subscribe(this);
    }

    protected override void OnUpdatePossessed()
    {
        // Get the inputs used during this frame
        Inputs inputs = NoControlInputs;

        if (ControlsEnabled())
        {
            inputs = FetchInputs();
        }

        UpdateDisplayInfo(inputs);
        UpdateMovement(inputs);
        UpdatePossession(inputs);
    }

    protected override Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();

        if (UseKeyboard)
        {
            // Inputs from the keyboard
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Possess = Input.GetButtonDown("Possess");
        }
        else
        {
            // Inputs from the controler
            inputs.Vertical = Input.GetAxisRaw("Vertical");
            inputs.Horizontal = Input.GetAxisRaw("Horizontal");
            inputs.Possess = Input.GetButtonDown("Possess");
        }

        return inputs;
    }

    protected override void OnUpdateNotPossessed() { }

    protected override Inputs CreateInputs()
    {
        return NoControlInputs;
    }
    
    protected override bool HasEnoughSpaceToUnpossess()
    {
        return LeftPlayerSpawn.OverlapCollider(LeftPlayerSpawnContactFilter, OverlapResults) == 0;
    }

    public override GameObject Unpossess(bool centerColliderToPos = false, Vector2? forceRespawnPos = null)
    {
        GameObject spawnedCharacter = null;

        if (IsPossessable && IsPossessed)
        {
            if (PossessingScript)
            {
                // Select the correct player spawn and respawn facing direction
                Vector2 respawnPos = LeftPlayerSpawn.transform.position;
                Vector2 respawnFacingDirection;

                if (transform.right.x < .0f)
                {
                    respawnFacingDirection = Vector2.left;
                }
                else
                {
                    respawnFacingDirection = Vector2.right;
                }

                if (forceRespawnPos != null)
                {
                    respawnPos = (Vector2)forceRespawnPos;
                }

                // Tell the possession script, that took possession of this AIController, that isn't in control anymore
                PossessingScript.ReleasePossession(respawnPos, respawnFacingDirection, true);

                spawnedCharacter = PossessingScript.gameObject;

                PossessingScript = null;
            }

            InfoUI.SetActive(false);

            IsPossessed = false;

            Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, .0f);
            
            AudioSource.pitch = Random.Range(.9f, 1.0f);
            AudioSource.PlayOneShot(OnUnpossessSound);

            OnUnpossess();
        }

        return spawnedCharacter;
    }

    private void UpdateMovement(Inputs inputs)
    {
        Vector2 aimingDirection = new Vector2(inputs.Horizontal, inputs.Vertical);

        if (aimingDirection.magnitude >= _minMagnitudeToAim)
        {
            _movementScript.SetAimingDirection(aimingDirection.normalized);
        }
    }

    // Methods of the IPhysicsCollision2DListener interface
    public void OnPhysicsCollision2DEnter(PhysicsCollision2D collision)
    {
        PossessionPower possessionPower = collision.GameObject.GetComponent<PossessionPower>();
        
        if (!possessionPower || (!possessionPower.InPossessionMode && PossessingScript != possessionPower))
        {
            _explodableBullet.Explode();
        }
    }

    public void OnPhysicsCollision2DStay(PhysicsCollision2D collision) { }

    public void OnPhysicsCollision2DExit(PhysicsCollision2D collision) { }

    // Methods of the IProximityExplodableSubscriber interface
    public void NotifyCountdownStarted(GameObject explodableGameObject, float timeRemaining) { }

    public void NotifyCountdownUpdated(GameObject explodableGameObject, float timeRemaining) { }

    public void NotifyCountdownFinished(GameObject explodableGameObject) { }

    public void NotifyExploded(GameObject explodableGameObject)
    {
        Unpossess(false, transform.position);

        VirtualCameraManager.Instance.RemoveVirtualCamera(PossessionVirtualCamera);
        Destroy(PossessionVirtualCamera.gameObject);
    }
}
