using UnityEngine;

public class FlyingSkullController : FlyingCharacterController
{
    [Header("Particules")]
    [SerializeField]
    private ParticleSystem _normalParticles;
    [SerializeField]
    private ParticleSystem _possessedParticles;

    protected override void Awake()
    {
        base.Awake();

        if (!_normalParticles)
        {
            Debug.LogError("No normal particle system was set for " + GetType() + " script of " + gameObject.name + "!");
        }

        if (!_possessedParticles)
        {
            Debug.LogError("No possessed particle system was set for " + GetType() + " script of " + gameObject.name + "!");
        }
        
        _normalParticles.Clear();

        _normalParticles.Play();
        _possessedParticles.Stop();
    }

    protected override void OnPossess(PossessionPower possessingScript)
    {
        _normalParticles.Stop();
        _possessedParticles.Play();

        _possessedParticles.Clear();
    }

    protected override void OnUnpossess()
    {
        _normalParticles.Play();
        _possessedParticles.Stop();

        _normalParticles.Clear();
    }
}
