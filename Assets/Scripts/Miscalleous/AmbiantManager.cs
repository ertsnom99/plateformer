using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AudioSource))]

public class AmbiantManager : KeptMonoSingleton<AmbiantManager>
{
    [Header("Ambiant")]
    [SerializeField]
    private AudioClip _generalAmbiant;
    [SerializeField]
    private float _generalAmbiantVolume = .8f;
    [SerializeField]
    private AudioClip _bossAmbiant;
    [SerializeField]
    private float _bossAmbiantVolume = .5f;

    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();

        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;
    }

    private void Start()
    {
        _audioSource.clip = _generalAmbiant;
        _audioSource.volume = _generalAmbiantVolume;
        _audioSource.Play();
    }

    public void StartBossAmbiant()
    {
        _audioSource.clip = _bossAmbiant;
        _audioSource.volume = _bossAmbiantVolume;
        _audioSource.Play();
    }
}
