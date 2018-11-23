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
    private AudioClip m_pickupSound;

    private SpriteRenderer m_spriteRenderer;
    private BoxCollider2D m_collider;
    private AudioSource m_audioSource;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_collider = GetComponent<BoxCollider2D>();
        m_audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == GameManager.PlayerTag)
        {
            foreach(IGemSubscriber subscriber in m_subscribers)
            {
                subscriber.NotifyGemCollected(this);
            }

            m_spriteRenderer.enabled = false;
            m_collider.enabled = false;

            PlayPickupSound();

            StartCoroutine(Destroy(m_pickupSound.length + .5f));
        }
    }

    private void PlayPickupSound()
    {
        m_audioSource.pitch = Random.Range(.9f, 1.0f);
        m_audioSource.PlayOneShot(m_pickupSound);
    }

    private IEnumerator Destroy(float destructionDelay)
    {
        yield return new WaitForSeconds(destructionDelay);
        Destroy(gameObject);
    }
}
