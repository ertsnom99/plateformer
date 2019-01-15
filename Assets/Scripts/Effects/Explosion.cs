using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AnimatorEventsManager))]
[RequireComponent(typeof(AudioSource))]

public class Explosion : MonoBehaviour, IAnimatorEventSubscriber
{
    [Header("Sound")]
    [SerializeField]
    private AudioClip m_explosionSound;

    private bool m_animationOver = false;
    private bool m_soundOver = false;

    private AnimatorEventsManager m_animatorEventsManager;
    private AudioSource m_audioSource;

    private void Awake()
    {
        m_animatorEventsManager = GetComponentInChildren<AnimatorEventsManager>();
        m_audioSource = GetComponentInChildren<AudioSource>();

        m_animatorEventsManager.Subscribe(AnimatorEvents.AnimationFinished, this);
    }

    private void Start()
    {
        if (m_explosionSound)
        {
            // Play sound
            m_audioSource.pitch = Random.Range(.9f, 1.0f);
            m_audioSource.PlayOneShot(m_explosionSound);
        }
        else
        {
            m_soundOver = true;
        }
    }


    private void Update()
    {
        if (!m_soundOver && !m_audioSource.isPlaying)
        {
            if (m_animationOver)
            {
                Destroy(gameObject);
            }
            else
            {
                m_soundOver = true;
            }
        }
    }

    // Methods of the IAnimatorEventSubscriber interface
    public void NotifyEvent(string eventName)
    {
        switch(eventName)
        {
            case AnimatorEvents.AnimationFinished:
                if (m_soundOver)
                {
                    Destroy(gameObject);
                }
                else
                {
                    m_animationOver = true;
                }

                break;
        }
    }
}
