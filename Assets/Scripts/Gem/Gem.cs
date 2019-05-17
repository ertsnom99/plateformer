using System.Collections;
using UnityEngine;

public interface IGemSubscriber
{
    void NotifyGemCollected(Gem collectedGem);
}

// This script requires thoses components and will be added if they aren't already there
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]

public class Gem : MonoSubscribable<IGemSubscriber>
{
    [Header("Sound")]
    [SerializeField]
    private AudioClip _pickupSound;

    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    private AudioSource _audioSource;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == GameManager.PlayerTag)
        {
            foreach(IGemSubscriber subscriber in Subscribers)
            {
                subscriber.NotifyGemCollected(this);
            }

            _spriteRenderer.enabled = false;
            _collider.enabled = false;

            PlayPickupSound();

            StartCoroutine(Destroy(_pickupSound.length + .5f));
        }
    }

    private void PlayPickupSound()
    {
        _audioSource.pitch = Random.Range(.9f, 1.0f);
        _audioSource.PlayOneShot(_pickupSound);
    }

    private IEnumerator Destroy(float destructionDelay)
    {
        yield return new WaitForSeconds(destructionDelay);
        Destroy(gameObject);
    }
}
