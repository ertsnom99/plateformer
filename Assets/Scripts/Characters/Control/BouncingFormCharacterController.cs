using Pathfinding;
using UnityEngine;

public interface IBouncingFormControllerSubscriber
{
    void NotifyPossessed(BouncingFormCharacterController possessedScript, PossessionPower possessingScript, PlayerController possessingController);
    void NotifyUnpossessed(BouncingFormCharacterController possessedScript);
    void NotifyCanceledBounce();
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(BouncingPhysicsObject))]

public class BouncingFormCharacterController : SubscribablePossessableCharacterController<IBouncingFormControllerSubscriber>
{
    private BouncingPhysicsObject _bouncingPhysics;

    protected override void Awake()
    {
        // Tells to use the layer settings from the Physics2D settings (the matrix)
        LeftPlayerSpawnContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(LeftPlayerSpawn.gameObject.layer));
        LeftPlayerSpawnContactFilter.useLayerMask = true;
        LeftPlayerSpawnContactFilter.useTriggers = false;

        IsPossessed = false;
        
        Animator = GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();
        Seeker = GetComponent<Seeker>();

        PossessedModeAnimationLayerIndex = Animator.GetLayerIndex(PossessedModeAnimationLayerName);
        Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, .0f);

        if (!LeftPlayerSpawn)
        {
            Debug.LogError("No left player spawn was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        _bouncingPhysics = GetComponent<BouncingPhysicsObject>();
    }

    protected override void OnUpdatePossessed()
    {
        if (ControlsEnabled())
        {
            if (PossessingController.CurrentInputs.ReleasePower)
            {
                CancelBounce();
            }

            UpdateDisplayInfo(PossessingController.CurrentInputs);
            UpdatePossession(PossessingController.CurrentInputs);
        }
        else
        {
            UpdateDisplayInfo(NoControlInputs);
            UpdatePossession(NoControlInputs);
        }
    }

    public void CancelBounce()
    {
        foreach (IBouncingFormControllerSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyCanceledBounce();
        }
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
    
    public override bool Possess(PossessionPower possessingScript, PlayerController possessingController)
    {
        if (IsPossessable && !IsPossessed)
        {
            PossessingScript = possessingScript;
            PossessingController = possessingController;

            IsPossessed = true;

            // When the bouncing form was taken possession of while it was actif
            if (gameObject.activeSelf)
            {
                Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, 1.0f);

                VirtualCameraManager.Instance.ChangeVirtualCamera(PossessionVirtualCamera);

                AudioSource.pitch = Random.Range(.9f, 1.0f);
                AudioSource.PlayOneShot(OnPossessSound);

                foreach (IBouncingFormControllerSubscriber subscriber in Subscribers)
                {
                    subscriber.NotifyPossessed(this, possessingScript, possessingController);
                }

                OnPossess(possessingScript);
            }
        }

        return IsPossessed;
    }

    public override GameObject Unpossess(bool centerColliderToPos = false, Vector2? forceRespawnPos = null)
    {
        GameObject spawnedCharacter = null;

        if (IsPossessable && IsPossessed)
        {
            if (gameObject.activeSelf)
            {
                if (PossessingScript)
                {
                    // Select the correct player spawn and respawn facing direction
                    Vector2 respawnPos = LeftPlayerSpawn.transform.position;
                    Vector2 respawnFacingDirection;

                     if (_bouncingPhysics.Velocity.x < .0f)
                     {
                         respawnFacingDirection = Vector2.left;
                     }
                     else
                     {
                         respawnFacingDirection = Vector2.right;
                     }

                    // Tell the possession script, that took possession of this AIController, that isn't in control anymore
                    PossessingScript.ReleasePossession(respawnPos, respawnFacingDirection, true);

                    spawnedCharacter = PossessingScript.gameObject;

                    PossessingController = null;
                    PossessingScript = null;
                }

                AudioSource.pitch = Random.Range(.9f, 1.0f);
                AudioSource.PlayOneShot(OnUnpossessSound);

                foreach (IBouncingFormControllerSubscriber subscriber in Subscribers)
                {
                    subscriber.NotifyUnpossessed(this);
                }

                OnUnpossess();
            }

            InfoUI.SetActive(false);

            IsPossessed = false;

            Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, .0f);
        }

        return spawnedCharacter;
    }

    private void OnEnable()
    {
        // The layer weight can't be changed if the gameobject isn't active
        Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, IsPossessed ? 1.0f : .0f);
    }
}
