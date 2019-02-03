using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AudioSource))]

public class PlayerHealth : Health
{
    [Header("Sound")]
    [SerializeField]
    private AudioClip m_damagedSound;
    [SerializeField]
    private AudioClip m_healedSound;

    private AudioSource m_audioSource;

    protected override void Awake()
    {
        base.Awake();

        m_audioSource = GetComponent<AudioSource>();
    }

    protected override void OnDamageApplied()
    {
        // Play sound
        m_audioSource.pitch = Random.Range(.9f, 1.0f);
        m_audioSource.PlayOneShot(m_damagedSound);
    }
    
    protected override void OnHealed()
    {
        // Play sound
        m_audioSource.pitch = Random.Range(.9f, 1.0f);
        m_audioSource.PlayOneShot(m_healedSound);
    }
}
