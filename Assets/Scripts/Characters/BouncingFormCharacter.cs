using UnityEngine;

public interface IBouncingFormSubscriber
{
    void NotifyPossessed(Controller controller, Bounds unpossessBounds, LayerMask unpossessCollisionMask, PossessablePawn.UnpossessCallbackDelegate onUnpossessCallback);
    void NotifyUnpossessed();
    void NotifyCanceledBounce();
}

public class BouncingFormCharacter : SubscribablePossessablePawn<IBouncingFormSubscriber>
{
    [SerializeField]
    private Collider2D _spawnArea;

    protected override void Awake()
    {
        base.Awake();
#if UNITY_EDITOR
        if (!_spawnArea)
        {
            Debug.LogError("No spawn area was set for " + GetType() + " script of " + gameObject.name + "!");
        }
#endif
    }

    public override void UpdateWithInputs(Inputs inputs)
    {
        if (inputs.ReleasePower)
        {
            CancelBounce();
        }

        base.UpdateWithInputs(inputs);
    }

    public void CancelBounce()
    {
        foreach (IBouncingFormSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyCanceledBounce();
        }
    }

    public void UnpossessWithoutSpawn()
    {
        if (IsPossessable && IsPossessed)
        {
            IsPossessed = false;
            UpdateVisual();
            OnUnpossessCallback = null;
        }
    }

    protected override bool HasEnoughSpaceToUnpossess(ref Vector2 spawnPosition, ref Vector2 spawnFacingDirection)
    {
        spawnPosition = _spawnArea.transform.position;
        spawnFacingDirection = transform.right.x < .0f ? Vector2.left : Vector2.right;
        return _spawnArea.OverlapCollider(ContactFilter, new Collider2D[1]) <= 0;
    }

    protected override void OnPossess()
    {
        foreach (IBouncingFormSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyPossessed(Controller, UnpossessBounds, ContactFilter.layerMask, OnUnpossessCallback);
        }
    }

    protected override void OnUnpossess()
    {
        foreach (IBouncingFormSubscriber subscriber in Subscribers)
        {
            subscriber.NotifyUnpossessed();
        }
    }

    public void OnEnable()
    {
        UpdateVisual();
    }
}
