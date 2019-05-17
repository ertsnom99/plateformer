using UnityEngine;

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(AnimatorEventsManager))]
[RequireComponent(typeof(AudioSource))]

public class Explosion : MonoBehaviour, IAnimatorEventSubscriber
{
    [Header("Sound")]
    [SerializeField]
    private AudioClip _explosionSound;

    private bool _animationOver = false;
    private bool _soundOver = false;

    private AnimatorEventsManager _animatorEventsManager;
    private AudioSource _audioSource;

    private void Awake()
    {
        _animatorEventsManager = GetComponentInChildren<AnimatorEventsManager>();
        _audioSource = GetComponentInChildren<AudioSource>();

        _animatorEventsManager.Subscribe(AnimatorEvents.AnimationFinish, this);
    }

    private void Start()
    {
        if (_explosionSound)
        {
            // Play sound
            _audioSource.pitch = Random.Range(.9f, 1.0f);
            _audioSource.PlayOneShot(_explosionSound);
        }
        else
        {
            _soundOver = true;
        }
    }
    
    private void Update()
    {
        if (!_soundOver && !_audioSource.isPlaying)
        {
            if (_animationOver)
            {
                Destroy(gameObject);
            }
            else
            {
                _soundOver = true;
            }
        }
    }

    // Methods of the IAnimatorEventSubscriber interface
    public void NotifyEvent(string eventName)
    {
        switch(eventName)
        {
            case AnimatorEvents.AnimationFinish:
                if (_soundOver)
                {
                    Destroy(gameObject);
                }
                else
                {
                    _animationOver = true;
                }

                break;
        }
    }
}
