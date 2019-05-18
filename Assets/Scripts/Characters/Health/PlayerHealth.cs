using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AudioSource))]

public class PlayerHealth : Health
{
    [Header("Sound")]
    [SerializeField]
    private AudioClip _damagedSound;
    [SerializeField]
    private AudioClip _healedSound;

    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();

        _audioSource = GetComponent<AudioSource>();
    }

    protected override void OnDamageDealt()
    {
        // Play sound
        _audioSource.pitch = Random.Range(.9f, 1.0f);
        _audioSource.PlayOneShot(_damagedSound);
    }
    
    protected override void OnHealingDone()
    {
        // Play sound
        _audioSource.pitch = Random.Range(.9f, 1.0f);
        _audioSource.PlayOneShot(_healedSound);
    }
}
