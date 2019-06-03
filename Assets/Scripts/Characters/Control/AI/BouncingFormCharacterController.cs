using Cinemachine;
using UnityEngine;

public interface IBouncingFormControllerSubscriber
{
    void NotifyPossessed(Possession possessingScript);
    void NotifyUnpossessed();
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(BouncingPhysicsObject))]

public class BouncingFormCharacterController : SubscribablePossessableCharacterController<IBouncingFormControllerSubscriber>
{
    private Possession _possessingScript;

    private BouncingPhysicsObject _bouncingPhysics;

    protected override void Awake()
    {
        // Tells to use the layer settings from the Physics2D settings (the matrix)
        LeftPlayerSpawnContactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(LeftPlayerSpawn.gameObject.layer));
        LeftPlayerSpawnContactFilter.useLayerMask = true;
        LeftPlayerSpawnContactFilter.useTriggers = false;

        IsPossessed = false;

        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();

        PossessedModeAnimationLayerIndex = Animator.GetLayerIndex(PossessedModeAnimationLayerName);
        Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, .0f);

        if (!LeftPlayerSpawn)
        {
            Debug.LogError("No left player spawn was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        _bouncingPhysics = GetComponent<BouncingPhysicsObject>();
    }

    protected override Inputs FetchInputs()
    {
        Inputs inputs = new Inputs();

        if (UseKeyboard)
        {
            // Inputs from the keyboard
            inputs.Possess = Input.GetButtonDown("Possess");
        }
        else
        {
            // TODO: Create inputs specific to the controler
            // Inputs from the controler
            inputs.Possess = Input.GetButtonDown("Possess");
        }

        return inputs;
    }

    protected override void OnUpdateNotPossessed() { }

    protected override Inputs CreateInputs()
    {
        return NoControlInputs;
    }

    // Returns if the area to respawn the player is free. The area checked is based on the facing direction of the character
    protected override bool HasEnoughSpaceToUnpossess()
    {
        return LeftPlayerSpawn.OverlapCollider(LeftPlayerSpawnContactFilter, OverlapResults) == 0;
    }

    public void SetPossessionVirtualCamera(CinemachineVirtualCamera possessionVirtualCamera)
    {
        PossessionVirtualCamera = possessionVirtualCamera;
    }

    // Methods of the IPossessable interface
    public override bool Possess(Possession possessingScript)
    {
        if (IsPossessable && !IsPossessed)
        {
            _possessingScript = possessingScript;

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
                    subscriber.NotifyPossessed(possessingScript);
                }
            }
        }

        return IsPossessed;
    }

    public override bool Unpossess()
    {
        if (IsPossessed && IsPossessed)
        {
            IsPossessed = false;

            Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, .0f);

            if (gameObject.activeSelf)
            {
                if (_possessingScript)
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
                    _possessingScript.ReleasePossession(respawnPos, respawnFacingDirection, true);
                    
                    _possessingScript = null;
                }

                AudioSource.pitch = Random.Range(.9f, 1.0f);
                AudioSource.PlayOneShot(OnUnpossessSound);

                foreach (IBouncingFormControllerSubscriber subscriber in Subscribers)
                {
                    subscriber.NotifyUnpossessed();
                }
            }
        }

        return IsPossessed;
    }

    private void OnEnable()
    {
        // The layer weight can't be changed if the gameobject isn't active
        Animator.SetLayerWeight(PossessedModeAnimationLayerIndex, IsPossessed ? 1.0f : .0f);
    }
}
