using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AudioSource))]

public class AmbientManager : KeptMonoSingleton<AmbientManager>
{
    [Header("Ambiant")]
    [SerializeField]
    private AudioClip _generalAmbient;

    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();

        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;
    }

    private void Start()
    {
        _audioSource.clip = _generalAmbient;
        _audioSource.Play();
    }
}
