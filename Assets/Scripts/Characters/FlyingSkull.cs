using UnityEngine;

public class FlyingSkull : FlyingCharacter
{
    [Header("Particules")]
    [SerializeField]
    private ParticleSystem _normalParticles;
    [SerializeField]
    private ParticleSystem _possessedParticles;
#if UNITY_EDITOR
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
    }
#endif
    protected override void Start()
    {
        base.Start();

        _possessedParticles.Clear();
        _possessedParticles.Stop();
        _normalParticles.Play();
        _normalParticles.Clear();
    }

    protected override void OnPossess()
    {
        _normalParticles.Stop();
        _possessedParticles.Play();
        _possessedParticles.Clear();
    }

    protected override void OnUnpossess()
    {
        _possessedParticles.Stop();
        _normalParticles.Play();
        _normalParticles.Clear();
    }
}
