using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(BulletMovement))]
[RequireComponent(typeof(ExplodableBullet))]

public class BulletCharacter : PossessablePawn, IPhysicsCollision2DListener, IProximityExplodableSubscriber
{
    [SerializeField]
    private Collider2D _spawnArea;

    [Header("Controls")]
    [SerializeField]
    float _minMagnitudeToAim = .8f;

    [Header("Explosion")]
    [SerializeField]
    private Transform _unpossessPositionOnExploded;

    private BulletMovement _movementScript;
    private ExplodableBullet _explodableBullet;

    protected override void Awake()
    {
        base.Awake();
#if UNITY_EDITOR
        if (!_spawnArea)
        {
            Debug.LogError("No spawn area was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_unpossessPositionOnExploded)
        {
            Debug.LogError("No unpossess position on exploded was set for " + GetType() + " script of " + gameObject.name + "!");
        }
#endif
        _movementScript = GetComponent<BulletMovement>();
        _explodableBullet = GetComponent<ExplodableBullet>();

        _explodableBullet.Subscribe(this);
    }

    public override void UpdateWithInputs(Inputs inputs)
    {
        UpdateMovement(inputs);
        base.UpdateWithInputs(inputs);
    }

    protected override bool HasEnoughSpaceToUnpossess(ref Vector2 spawnPosition, ref Vector2 spawnFacingDirection)
    {
        spawnPosition = _spawnArea.transform.position;
        spawnFacingDirection = transform.right.x < .0f ? Vector2.left : Vector2.right;
        return _spawnArea.OverlapCollider(ContactFilter, new Collider2D[1]) <= 0;
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
        // Bullet shouldn't explode when touching the player in possess mode
        PossessionPower possessionPower = collision.GameObject.GetComponent<PossessionPower>();
        
        if (IsPossessed || !possessionPower || !possessionPower.InPossessionMode)
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
        Unpossess(_spawnArea.transform.position, transform.right.x < .0f ? Vector2.left : Vector2.right);

        VirtualCameraManager.Instance.RemoveVirtualCamera(PossessionVirtualCamera);
        Destroy(PossessionVirtualCamera.gameObject);
    }
}
