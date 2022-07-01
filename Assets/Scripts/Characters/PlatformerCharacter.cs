using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(PlatformerMovement))]
[RequireComponent(typeof(ExplodableCharacter))]

public class PlatformerCharacter : PossessablePawn, IProximityExplodableSubscriber
{
    private PlatformerMovement _movementScript;
    private ExplodableCharacter _explodableCharacterScript;

    protected override void Awake()
    {
        base.Awake();

        _movementScript = GetComponent<PlatformerMovement>();
        _explodableCharacterScript = GetComponent<ExplodableCharacter>();

        SetIsLookingForwardDelegate(_movementScript.IsLookingForward);

        _explodableCharacterScript.Subscribe(this);
    }

    public override void UpdateWithInputs(Inputs inputs)
    {
        UpdateMovement(inputs);
        UpdateExplosion(inputs);
        base.UpdateWithInputs(inputs);
    }

    private void UpdateMovement(Inputs inputs)
    {
        _movementScript.SetInputs(inputs);
        _movementScript.UpdateMovement();
    }

    private void UpdateExplosion(Inputs inputs)
    {
        if (inputs.PressPower && !_explodableCharacterScript.CountdownStarted)
        {
            _explodableCharacterScript.StartCountdown();
        }
    }
    
    // Methods of the IProximityExplodableSubscriber interface
    public void NotifyCountdownStarted(GameObject explodableGameObject, float timeRemaining) { }

    public void NotifyCountdownUpdated(GameObject explodableGameObject, float timeRemaining) { }

    public void NotifyCountdownFinished(GameObject explodableGameObject) { }

    public void NotifyExploded(GameObject explodableGameObject)
    {
        Vector2 spawnPosition = new Vector2(transform.position.x, CalculateSpawnY());
        Unpossess(spawnPosition, IsLookingForward() ? Vector2.right : Vector2.left);
    }
}
