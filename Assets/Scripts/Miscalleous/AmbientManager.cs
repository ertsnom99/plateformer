using System.Collections;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AudioSource))]

public class AmbientManager : KeptMonoSingleton<AmbientManager>
{
    [Header("Ambiant")]
    [SerializeField]
    private AudioClip _generalAmbient;

    private AudioSource _audioSource;
    private float _initialVolume;

    public bool IsVolumeFading { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;

        _initialVolume = _audioSource.volume;
    }

    private void Start()
    {
        _audioSource.clip = _generalAmbient;
        _audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        _audioSource.volume = volume;
    }

    public void FadeVolumeIn(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeVolume(_audioSource.volume, _initialVolume, duration));

        IsVolumeFading = true;
    }

    public void FadeVolumeOut(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeVolume(_audioSource.volume, .0f, duration));

        IsVolumeFading = true;
    }

    IEnumerator FadeVolume(float from, float to, float duration)
    {
        float volume = _audioSource.volume;

        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            volume = Mathf.Lerp(from, to, (Time.time - startTime) / duration);
            _audioSource.volume = volume;

            yield return 0;
        }

        volume = to;
        _audioSource.volume = volume;

        if (to == _initialVolume)
        {
            IsVolumeFading = false;
        }
        else if (to == .0f)
        {
            IsVolumeFading = false;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
